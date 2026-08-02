using komikaan.Common.Models;
using komikaan.GTFS.Models.Static;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Adapters;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Contexts;

public class GTFSContext
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<GTFSContext> _logger;

    public GTFSContext(IConfiguration configuration, ILogger<GTFSContext> logger)
    {
        _logger = logger;
        // Get the connection string from configuration
        var connectionString = configuration.GetConnectionString("gtfs");

        // Build the NpgsqlDataSource
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        // Map composite types for each entity (these are the custom types in your PostgreSQL DB)
        builder.MapComposite<PSQLTrip>("public.trips_type");
        builder.MapComposite<PSQLAgency>("public.agencies_type");
        builder.MapComposite<PSQLRoute>("public.routes_type");
        builder.MapComposite<PSQLStop>("public.stops_type");
        builder.MapComposite<PSQLCalendar>("public.calendars_type");
        builder.MapComposite<PSQLCalendarDate>("public.calendar_dates_type");
        //builder.MapComposite<Frequency>("public.frequencies_type");
        builder.MapComposite<PSQLStopTime>("public.stop_times_type");
        builder.MapComposite<PSQLShape>("public.shapes_type");

        // Build the NpgsqlDataSource
        _dataSource = builder.Build();
    }

    private async Task UpsertEntityAsync<T>(ImportRequest supplierConfig, string procedureName, string tvpTypeName, IAsyncEnumerable<T> entities, int batchSize, bool partioned, CancellationToken cancellationToken) where T : GTFSStaticObject
    {
        var sanatizedSupplierName = supplierConfig.Name.ToString().Replace("-", "_").Replace(" ", "_").Replace(".", "_");
        var partitionName = $"{sanatizedSupplierName}_{supplierConfig.QueuedImportId.ToString().Replace("-", "_")}";
        partitionName = partitionName.Length <= 62 ? partitionName : partitionName.Substring(0, 62);

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Importing to {procedure}", procedureName);
        _logger.LogInformation("Chunk size configured as {size}", batchSize);

        var totalGrabbed = 0;
        var hasData = false;
        var partitionCreated = false;
        
        var chunks = entities.Chunk(batchSize);
        
        await foreach (var chunk in chunks.WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            hasData = true;

            var chunkToInsert = chunk;
            if (procedureName == "public.upsert_stops" && typeof(T) == typeof(PSQLStop))
            {
                var dedupedStops = chunk
                    .Cast<PSQLStop>()
                    .GroupBy(stop => (stop.DataOrigin ?? string.Empty, stop.Id ?? string.Empty))
                    .Select(group => group.Last())
                    .ToArray();

                if (dedupedStops.Length != chunk.Length)
                {
                    _logger.LogWarning(
                        "Filtered {duplicates} duplicate stops in chunk {grab} for {procedureName}",
                        chunk.Length - dedupedStops.Length,
                        totalGrabbed + 1,
                        procedureName);
                }

                if (dedupedStops.Length == 0)
                {
                    totalGrabbed += 1;
                    continue;
                }

                chunkToInsert = dedupedStops.Cast<T>().ToArray();
            }

            if (partioned && !partitionCreated)
            {
                _logger.LogInformation("Creating a partition");

                using (var connection = _dataSource.CreateConnection())
                {
                    var query = $"CREATE TABLE IF NOT EXISTS public.\"{partitionName}\" PARTITION OF public.stop_times\n";
                    query += $"FOR VALUES FROM ('{supplierConfig.Name}', '{supplierConfig.QueuedImportId}')\n";
                    query += $"TO ('{supplierConfig.Name}', '{supplierConfig.QueuedImportId.Increment()}')\n";

                    _logger.LogInformation("Generated query: {query}", query);
                    var command = new NpgsqlCommand(query, connection);
                    await connection.OpenAsync(cancellationToken);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                partitionCreated = true;
            }

            var chunkWatch = Stopwatch.StartNew();
            totalGrabbed += 1;
            _logger.LogInformation("Working on chunk {grab} for {procedureName}", totalGrabbed, procedureName);

            using (var connection = _dataSource.CreateConnection())
            {
                var command = new NpgsqlCommand($"CALL {procedureName}(@items)", connection);
                await connection.OpenAsync(cancellationToken);

                var parameter = command.Parameters.AddWithValue("@items", chunkToInsert);
                parameter.DataTypeName = tvpTypeName + "[]";

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            _logger.LogInformation("Inserted chunk {grab} for {procedureName} in {time}", totalGrabbed, procedureName, chunkWatch.Elapsed);
        }

        if (partioned && hasData)
        {
            _logger.LogInformation("Deleting irrelevant partitions");

            using (var connection = _dataSource.CreateConnection())
            {
                var query = $@"DO $$ 
    DECLARE
        partition RECORD;
    BEGIN
        -- Loop through all partitions of the stop_times table
        FOR partition IN 
            SELECT tablename
            FROM pg_tables
            WHERE schemaname = 'public'
    		AND lower(tablename) LIKE '{sanatizedSupplierName}_%'
    		AND lower(tablename) NOT LIKE 'stop_times_default'
            AND lower(tablename) NOT LIKE '{partitionName}'
        LOOP
            -- Dynamically drop each partition
            EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(partition.tablename);
        END LOOP;
    END $$;".ToLower();

                _logger.LogInformation("Generated query: {query}", query);
                var command = new NpgsqlCommand(query, connection);
                await connection.OpenAsync(cancellationToken);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        _logger.LogInformation("Finished importing to {procedure} in {time}", procedureName, stopwatch.Elapsed);
    }

    //    // Bulk upsert for agencies
    public Task UpsertAgenciesAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLAgency> agencies, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_agencies";
        const string tvpTypeName = "public.agencies_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, agencies, 100, false, cancellationToken);
    }

    //    // Bulk upsert for routes
    public Task UpsertRoutesAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLRoute> routes, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_routes";
        const string tvpTypeName = "public.routes_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, routes, 5000, false, cancellationToken);
    }

    public Task UpsertCalendarsAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLCalendar> calenders, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_calendars";
        const string tvpTypeName = "public.calendars_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, calenders, 5000, false, cancellationToken);
    }

    // Bulk upsert for stops
    public Task UpsertStopsAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLStop> stops, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_stops";
        const string tvpTypeName = "public.stops_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, stops, 1000, false, cancellationToken);
    }

    //    // Bulk upsert for trips
    public Task UpsertTripsAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLTrip> trips, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_trips";
        const string tvpTypeName = "public.trips_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, trips, 10000, false, cancellationToken);
    }

    // Bulk upsert for calendar dates
    public Task UpsertCalendarDatesAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLCalendarDate> calendarDates, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_calendar_dates";
        const string tvpTypeName = "public.calendar_dates_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, calendarDates, 100000, false, cancellationToken);
    }

    //    // Bulk upsert for frequencies
    //    public async Task UpsertFrequenciesAsync(IEnumerable<Frequency> frequencies)
    //    {
    //        if (frequencies.Any())
    //        {
    //            const string procedureName = "public.upsert_frequencies";
    //            const string tvpTypeName = "public.frequencies_type";
    //            var item = frequencies.First();
    //            await UpsertEntityAsync(procedureName, tvpTypeName, frequencies, 5000, false);
    //        }
    //    }

    //    // Bulk upsert for stop times
    public Task UpsertStopTimesAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLStopTime> stopTimes, CancellationToken cancellationToken = default)
    {
        return UpsertEntityAsync(supplierConfig, "public.upsert_stop_times", "public.stop_times_type", stopTimes, 100000, true, cancellationToken);
    }

    //    // Bulk upsert for shapes
    public Task UpsertShapesAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLShape> shapes, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_shapes";
        const string tvpTypeName = "public.shapes_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, shapes, 100000, false, cancellationToken);
    }
}

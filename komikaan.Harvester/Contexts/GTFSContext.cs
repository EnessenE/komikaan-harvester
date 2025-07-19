using komikaan.Common.Models;
using komikaan.GTFS.Models.Static;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Adapters;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;

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


    private async Task UpsertEntityAsync<T>(SupplierConfiguration supplierConfig, string procedureName, string tvpTypeName, IEnumerable<T> entities, int batchSize, bool partioned) where T : GTFSStaticObject
    {
        if (partioned)
        {
            _logger.LogInformation("Creating a partition");
            var item = entities.First();

            using (var connection = _dataSource.CreateConnection())
            {
                var query = $"CREATE TABLE IF NOT EXISTS public.stop_times2_{supplierConfig.Name.ToString().Replace("-", "_").Replace(" ", "_").Replace(".", "_")}_{supplierConfig.ImportId.ToString().Replace("-", "_")} PARTITION OF public.stop_times2\n";
                query += $"FOR VALUES FROM ('{supplierConfig.Name}', '{supplierConfig.ImportId}')\n";
                query += $"TO ('{supplierConfig.Name}', '{supplierConfig.ImportId.Increment()}')\n";

                _logger.LogInformation("Generated query: {query}", query);
                var command = new NpgsqlCommand(query, connection);
                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Importing to {procedure}", procedureName);
        var chunks = entities.Chunk(batchSize).ToList();
        _logger.LogInformation("Split into {amount} of chunks of {size}", chunks.Count, batchSize);
        var totalGrabbed = 0;

        foreach (var chunk in chunks)
        {
            var chunkWatch = Stopwatch.StartNew();
            totalGrabbed += 1;
            _logger.LogInformation("Working on {grab}/{total} for {procedureName}", totalGrabbed, chunks.Count(), procedureName);

            using (var connection = _dataSource.CreateConnection())
            {

                var command = new NpgsqlCommand($"CALL {procedureName}(@items)", connection);
                await connection.OpenAsync();

                var parameter = command.Parameters.AddWithValue("@items", chunk);
                parameter.DataTypeName = tvpTypeName + "[]";  // Specify the array of custom composite type

                await command.ExecuteNonQueryAsync();
            }
            _logger.LogInformation("Inserted on {grab}/{total} for {procedureName} in {time}", totalGrabbed, chunks.Count(), procedureName, chunkWatch.Elapsed);
        }


        if (partioned)
        {
            _logger.LogInformation("Deleting irrelevant partitions");

            using (var connection = _dataSource.CreateConnection())
            {
                var query = $@"DO $$ 
    DECLARE
        partition RECORD;
    BEGIN
        -- Loop through all partitions of the stop_times2 table
        FOR partition IN 
            SELECT tablename
            FROM pg_tables
            WHERE schemaname = 'public'
    		AND tablename LIKE 'stop_times2_{supplierConfig.Name.ToString().Replace("-", "_").Replace(" ", "_").Replace(".", "_")}_%'
    		AND tablename NOT LIKE 'stop_times2_default'
            AND tablename NOT LIKE 'stop_times2_{supplierConfig.Name.ToString().Replace("-", "_").Replace(" ", "_").Replace(".", "_")}_{supplierConfig.ImportId.ToString().Replace("-", "_")}'
        LOOP
            -- Dynamically drop each partition
            EXECUTE 'DROP TABLE IF EXISTS public.' || partition.tablename;
        END LOOP;
    END $$;".ToLowerInvariant(); ;

                _logger.LogInformation("Generated query: {query}", query);
                var command = new NpgsqlCommand(query, connection);
                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }

        }
        _logger.LogInformation("Finished importing to {procedure} in {time}", procedureName, stopwatch.Elapsed);
    }



    //    // Bulk upsert for agencies
    public async Task UpsertAgenciesAsync(SupplierConfiguration supplierConfig, IEnumerable<PSQLAgency> agencies)
    {
        if (agencies.Any())
        {
            const string procedureName = "public.upsert_agencies";
            const string tvpTypeName = "public.agencies_type";
            await UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, agencies, 100, false);
        }
    }

    //    // Bulk upsert for routes
    public async Task UpsertRoutesAsync(SupplierConfiguration supplierConfig, IEnumerable<PSQLRoute> routes)
    {
        if (routes.Any())
        {
            const string procedureName = "public.upsert_routes";
            const string tvpTypeName = "public.routes_type";
            await UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, routes, 5000, false);
        }
    }

    public async Task UpsertCalendarsAsync(SupplierConfiguration supplierConfig, IEnumerable<PSQLCalendar> calenders)
    {
        if (calenders.Any())
        {
            const string procedureName = "public.upsert_calendars";
            const string tvpTypeName = "public.calendars_type";
            await UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, calenders, 5000, false);
        }
    }

    // Bulk upsert for stops
    public async Task UpsertStopsAsync(SupplierConfiguration supplierConfig, IEnumerable<PSQLStop> stops)
    {
        if (stops.Any())
        {
            const string procedureName = "public.upsert_stops";
            const string tvpTypeName = "public.stops_type";
            await UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, stops, 1000, false);
        }
    }

    //    // Bulk upsert for trips
    public async Task UpsertTripsAsync(SupplierConfiguration supplierConfig, IEnumerable<PSQLTrip> trips)
    {
        if (trips.Any())
        {
            const string procedureName = "public.upsert_trips";
            const string tvpTypeName = "public.trips_type";
            await UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, trips, 10000, false);
        }
    }

    // Bulk upsert for calendar dates
    public async Task UpsertCalendarDatesAsync(SupplierConfiguration supplierConfig, IEnumerable<PSQLCalendarDate> calendarDates)
    {
        if (calendarDates.Any())
        {
            const string procedureName = "public.upsert_calendar_dates";
            const string tvpTypeName = "public.calendar_dates_type";
            var item = calendarDates.First();
            await UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, calendarDates, 100000, false);
        }
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
    public async Task UpsertStopTimesAsync(SupplierConfiguration supplierConfig, IEnumerable<PSQLStopTime> stopTimes)
    {
        if (stopTimes.Any())
        {
            await UpsertEntityAsync(supplierConfig, "public.upsert_stop_times2", "public.stop_times_type", stopTimes, 100000, true);
            //await UpsertEntityAsync("public.upsert_stop_times", "public.stop_times_type", ToPsql(stopTimes), 100000, false);
        }
    }

    //    // Bulk upsert for shapes
    public async Task UpsertShapesAsync(SupplierConfiguration supplierConfig, IEnumerable<PSQLShape> shapes)
    {
        if (shapes.Any())
        {
            const string procedureName = "public.upsert_shapes";
            const string tvpTypeName = "public.shapes_type";
            var item = shapes.First();
            await UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, shapes, 100000, false);
        }
    }
}

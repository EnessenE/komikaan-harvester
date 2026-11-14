using komikaan.GTFS.Models.Static;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Adapters;
using Npgsql;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
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
        builder.MapComposite<PSQLFrequency>("public.frequencies_type");
        builder.MapComposite<PSQLStopTime>("public.stop_times_type");
        builder.MapComposite<PSQLShape>("public.shapes_type");

        // Build the NpgsqlDataSource
        _dataSource = builder.Build();
    }

    private async Task UpsertEntityAsync<T>(ImportRequest supplierConfig, string procedureName, string tvpTypeName, IAsyncEnumerable<T> entities, int batchSize, bool partioned, CancellationToken cancellationToken, Func<T, object>? dedupKey = null) where T : GTFSStaticObject
    {
        // Partition names must fit the 63-byte PG identifier limit; the import id (36 chars with
        // underscores) always sits at the end, so the prefix is capped at 25 chars
        var partitionPrefix = BuildPartitionPrefix(supplierConfig.Name);
        var partitionName = $"{partitionPrefix}_{supplierConfig.QueuedImportId.ToString().Replace("-", "_")}";
        // Escape for use inside a SQL string literal
        var escapedSupplierName = supplierConfig.Name.Replace("'", "''");

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Importing to {procedure}", procedureName);
        _logger.LogInformation("Chunk size configured as {size}", batchSize);

        var totalGrabbed = 0;
        var hasData = false;
        var partitionCreated = false;

        // One connection for the whole upsert instead of one per chunk
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await foreach (var chunk in entities.Chunk(batchSize).WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            hasData = true;

            var chunkToInsert = chunk;
            if (dedupKey != null)
            {
                // MERGE and ON CONFLICT DO UPDATE fail when a single statement contains the same
                // conflict key twice ("cannot affect row a second time"), so keep only the last
                // row per key inside each chunk
                var deduped = chunk
                    .GroupBy(dedupKey)
                    .Select(group => group.Last())
                    .ToArray();

                if (deduped.Length != chunk.Length)
                {
                    _logger.LogWarning(
                        "Filtered {duplicates} duplicate rows in chunk {grab} for {procedureName}",
                        chunk.Length - deduped.Length,
                        totalGrabbed + 1,
                        procedureName);
                }

                if (deduped.Length == 0)
                {
                    totalGrabbed += 1;
                    continue;
                }

                chunkToInsert = deduped;
            }

            if (partioned && !partitionCreated)
            {
                _logger.LogInformation("Creating a partition");

                var query = $@"CREATE TABLE IF NOT EXISTS public.""{partitionName}"" PARTITION OF public.stop_times ";
                query += $"FOR VALUES FROM ('{escapedSupplierName}', '{supplierConfig.QueuedImportId}')\n";
                query += $"TO ('{escapedSupplierName}', '{supplierConfig.QueuedImportId.Increment()}')\n";

                _logger.LogInformation("Generated query: {query}", query);
                await using (var command = new NpgsqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                partitionCreated = true;
            }

            var chunkWatch = Stopwatch.StartNew();
            totalGrabbed += 1;
            _logger.LogInformation("Working on chunk {grab} for {procedureName}", totalGrabbed, procedureName);

            await using (var command = new NpgsqlCommand($"CALL {procedureName}(@items)", connection))
            {
                var parameter = command.Parameters.AddWithValue("@items", chunkToInsert);
                parameter.DataTypeName = tvpTypeName + "[]";

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            _logger.LogInformation("Inserted chunk {grab} for {procedureName} in {time}", totalGrabbed, procedureName, chunkWatch.Elapsed);
        }

        if (partioned && hasData)
        {
            _logger.LogInformation("Deleting irrelevant partitions");

            // Match case-insensitively (pg_tables keeps the quoted casing) and escape LIKE wildcards,
            // supplier names become '_' in the partition name which is a LIKE wildcard itself
            var likePattern = EscapeLike(partitionPrefix.ToLowerInvariant() + "_") + "%"; // trailing % stays a wildcard
            var query = $@"DO $$
    DECLARE
        partition RECORD;
    BEGIN
        -- Loop through all partitions of the stop_times table
        FOR partition IN
            SELECT tablename
            FROM pg_tables
            WHERE schemaname = 'public'
    		AND lower(tablename) LIKE '{likePattern}' ESCAPE '\'
    		AND lower(tablename) NOT LIKE 'stop\_times\_default' ESCAPE '\'
            AND lower(tablename) <> '{partitionName.ToLowerInvariant()}'
        LOOP
            -- Dynamically drop each partition
            EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(partition.tablename);
        END LOOP;
    END $$;";

            _logger.LogInformation("Generated query: {query}", query);
            await using (var command = new NpgsqlCommand(query, connection))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        _logger.LogInformation("Finished importing to {procedure} in {time}", procedureName, stopwatch.Elapsed);
    }

    private static string SanitizeIdentifier(string name)
    {
        var chars = name.Select(c => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') ? c : '_').ToArray();
        return new string(chars);
    }

    /// <summary>
    /// Prefix for stop_times partition names. When sanitization changed the name, a short stable
    /// hash of the raw name is appended so suppliers like "gob.es" and "gob_es" can never share
    /// (and drop) each other's partitions. Capped at 25 chars so prefix + '_' + import id
    /// (36 chars) always fits the 63-byte identifier limit.
    /// </summary>
    private static string BuildPartitionPrefix(string rawName)
    {
        var sanitized = SanitizeIdentifier(rawName);
        var prefix = sanitized == rawName
            ? sanitized
            : $"{sanitized}_{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawName)))[..6].ToLowerInvariant()}";
        const int maxPrefixLength = 62 - 36 - 1;
        return prefix.Length <= maxPrefixLength ? prefix : prefix.Substring(0, maxPrefixLength);
    }

    private static string EscapeLike(string value)
    {
        return value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }

    //    // Bulk upsert for agencies
    public Task UpsertAgenciesAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLAgency> agencies, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_agencies";
        const string tvpTypeName = "public.agencies_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, agencies, 100, false, cancellationToken, a => (a.DataOrigin, a.Id));
    }

    //    // Bulk upsert for routes
    public Task UpsertRoutesAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLRoute> routes, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_routes";
        const string tvpTypeName = "public.routes_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, routes, 5000, false, cancellationToken, r => (r.DataOrigin, r.Id));
    }

    public Task UpsertCalendarsAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLCalendar> calenders, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_calendars";
        const string tvpTypeName = "public.calendars_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, calenders, 5000, false, cancellationToken, c => (c.DataOrigin, c.ServiceId));
    }

    // Bulk upsert for stops
    public Task UpsertStopsAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLStop> stops, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_stops";
        const string tvpTypeName = "public.stops_type";
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, stops, 1000, false, cancellationToken, s => (s.DataOrigin, s.Id));
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

    // Bulk upsert for frequencies
    public Task UpsertFrequenciesAsync(ImportRequest supplierConfig, IAsyncEnumerable<PSQLFrequency> frequencies, CancellationToken cancellationToken = default)
    {
        const string procedureName = "public.upsert_frequencies";
        const string tvpTypeName = "public.frequencies_type";
        // Key mirrors the ON CONFLICT (data_origin, trip_id, start_time, end_time); the wrapped
        // time-of-day values are used because that is what lands in the time columns
        return UpsertEntityAsync(supplierConfig, procedureName, tvpTypeName, frequencies, 5000, false, cancellationToken, f => (f.DataOrigin, f.TripId, f.StartTimeData, f.EndTimeData));
    }

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

using GTFS.Entities;
using Npgsql;
using Route = GTFS.Entities.Route;
using System.Diagnostics;
using komikaan.Harvester.Contexts.ORM;

namespace komikaan.Harvester.Contexts;

internal class GTFSContext
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
        builder.MapComposite<PsqlTrip>("public.trips_type");
        builder.MapComposite<Agency>("public.agencies_type");
        builder.MapComposite<PsqlRoute>("public.routes_type");
        builder.MapComposite<PsqlStop>("public.stops_type");
        builder.MapComposite<Calendar>("public.calenders_type");
        builder.MapComposite<PsqlCalendarDate>("public.calendar_dates_type");
        builder.MapComposite<Frequency>("public.frequencies_type");
        builder.MapComposite<PsqlStopTime>("public.stop_times_type");
        builder.MapComposite<PsqlShape>("public.shapes_type");

        // Build the NpgsqlDataSource
        _dataSource = builder.Build();
    }


    private async Task UpsertEntityAsync<T>(string procedureName, string tvpTypeName, IEnumerable<T> entities, int batchSize, bool partioned) where T : GTFSEntity
    {
        if (partioned)
        {
            _logger.LogInformation("Creating a partition");
            var item = entities.First();
            
            using (var connection = _dataSource.CreateConnection())
            {
                var query = $"CREATE TABLE IF NOT EXISTS public.stop_times2_{item.DataOrigin.ToString().Replace("-", "_").Replace(" ", "_").Replace(".", "_")}_{item.ImportId.Value.ToString().Replace("-", "_")} PARTITION OF public.stop_times2\n";
                query += $"FOR VALUES FROM ('{item.DataOrigin}', '{item.ImportId.Value}')\n";
                query += $"TO ('{item.DataOrigin}', '{item.ImportId.Value.Increment()}')\n";

                _logger.LogInformation("Generated query: {query}", query);
                var command = new NpgsqlCommand(query, connection);
                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Importing to {procedure}", procedureName);
        var chunks = entities.Chunk(batchSize);
        _logger.LogInformation("Split into {amount} of chunks of {size}", chunks.Count(), batchSize);
        var totalGrabbed = 0;

        foreach (var chunk in chunks.ToList())
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

            var item = entities.First();

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
		AND tablename LIKE 'stop_times2_{item.DataOrigin.ToString().Replace("-", "_").Replace(" ", "_").Replace(".", "_")}_%'
		AND tablename NOT LIKE 'stop_times2_default'
        AND tablename NOT LIKE 'stop_times2_{item.DataOrigin.ToString().Replace("-", "_").Replace(" ", "_").Replace(".", "_")}_{item.ImportId.Value.ToString().Replace("-", "_")}'
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



    // Bulk upsert for agencies
    public async Task UpsertAgenciesAsync(IEnumerable<Agency> agencies)
    {
        if (agencies.Any())
        {
            const string procedureName = "public.upsert_agencies";
            const string tvpTypeName = "public.agencies_type";
            var item = agencies.First();
            await UpsertEntityAsync(procedureName, tvpTypeName, agencies, 100, false);
        }
    }

    // Bulk upsert for routes
    public async Task UpsertRoutesAsync(IEnumerable<Route> routes)
    {
        if (routes.Any())
        {
            const string procedureName = "public.upsert_routes";
            const string tvpTypeName = "public.routes_type";
            var item = routes.First();
            await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(routes), 5000, false);
        }
    }

    public async Task UpsertCalendarsAsync(IEnumerable<Calendar> calenders)
    {
        if (calenders.Any())
        {
            const string procedureName = "public.upsert_calenders";
            const string tvpTypeName = "public.calenders_type";
            var item = calenders.First();
            await UpsertEntityAsync(procedureName, tvpTypeName, calenders, 5000, false);
        }
    }

    // Bulk upsert for stops
    public async Task UpsertStopsAsync(IEnumerable<Stop> stops)
    {
        if (stops.Any())
        {
            const string procedureName = "public.upsert_stops";
            const string tvpTypeName = "public.stops_type";
            var item = stops.First();
            await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(stops), 1000, false);
        }
    }

    // Bulk upsert for trips
    public async Task UpsertTripsAsync(IEnumerable<Trip> trips)
    {
        if (trips.Any())
        {
            const string procedureName = "public.upsert_trips";
            const string tvpTypeName = "public.trips_type";
            var item = trips.First();
            await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(trips), 10000, false);
        }
    }

    // Bulk upsert for calendar dates
    public async Task UpsertCalendarDatesAsync(IEnumerable<CalendarDate> calendarDates)
    {
        if (calendarDates.Any())
        {
            const string procedureName = "public.upsert_calendar_dates";
            const string tvpTypeName = "public.calendar_dates_type";
            var item = calendarDates.First();
            await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(calendarDates), 100000, false);
        }
    }

    // Bulk upsert for frequencies
    public async Task UpsertFrequenciesAsync(IEnumerable<Frequency> frequencies)
    {
        if (frequencies.Any())
        {
            const string procedureName = "public.upsert_frequencies";
            const string tvpTypeName = "public.frequencies_type";
            var item = frequencies.First();
            await UpsertEntityAsync(procedureName, tvpTypeName, frequencies, 5000, false);
        }
    }

    // Bulk upsert for stop times
    public async Task UpsertStopTimesAsync(IEnumerable<StopTime> stopTimes)
    {
        if (stopTimes.Any())
        {
            var item = stopTimes.First();
            await UpsertEntityAsync("public.upsert_stop_times2", "public.stop_times_type", ToPsql(stopTimes), 100000, true);
            //await UpsertEntityAsync("public.upsert_stop_times", "public.stop_times_type", ToPsql(stopTimes), 100000, false);
        }
    }

    // Bulk upsert for shapes
    public async Task UpsertShapesAsync(IEnumerable<Shape> shapes)
    {
        if (shapes.Any())
        {
            const string procedureName = "public.upsert_shapes";
            const string tvpTypeName = "public.shapes_type";
            var item = shapes.First();
            await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(shapes), 100000, false);
        }
    }


    private static IEnumerable<PsqlStopTime> ToPsql(IEnumerable<StopTime> items)
    {
        foreach (var item in items)
        {
            var psqlItem = new PsqlStopTime(item);
            yield return psqlItem;
        }
    }
    private static IEnumerable<PsqlRoute> ToPsql(IEnumerable<Route> items)
    {
        foreach (var item in items)
        {
            var psqlItem = new PsqlRoute(item);
            yield return psqlItem;
        }
    }

    private static IEnumerable<PsqlTrip> ToPsql(IEnumerable<Trip> items)
    {
        foreach (var item in items)
        {
            var psqlItem = new PsqlTrip(item);
            yield return psqlItem;
        }
    }

    private static IEnumerable<PsqlStop> ToPsql(IEnumerable<Stop> items)
    {
        foreach (var item in items)
        {
            var psqlItem = new PsqlStop(item);
            yield return psqlItem;
        }
    }

    private static IEnumerable<PsqlCalendarDate> ToPsql(IEnumerable<CalendarDate> items)
    {
        foreach (var item in items)
        {
            var psqlItem = new PsqlCalendarDate(item);
            yield return psqlItem;
        }
    }


    private static IEnumerable<PsqlShape> ToPsql(IEnumerable<Shape> items)
    {
        foreach (var item in items)
        {
            var psqlItem = new PsqlShape(item);
            yield return psqlItem;
        }
    }

}
//dumb test delete thanks, make proper partitions idiot
static class GuidExtensions
{
    private static readonly int[] _guidByteOrder =
        new[] { 15, 14, 13, 12, 11, 10, 9, 8, 6, 7, 4, 5, 0, 1, 2, 3 };
    public static Guid Increment(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        bool carry = true;
        for (int i = 0; i < _guidByteOrder.Length && carry; i++)
        {
            int index = _guidByteOrder[i];
            byte oldValue = bytes[index]++;
            carry = oldValue > bytes[index];
        }
        return new Guid(bytes);
    }
}
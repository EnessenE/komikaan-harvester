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


    private async Task UpsertEntityAsync<T>(string procedureName, string tvpTypeName, IEnumerable<T> entities, int batchSize)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Importing to {procedure}", procedureName);
        var chunks = entities.Chunk(batchSize);
        _logger.LogInformation("Split into {amount} of chunks of {size}", chunks.Count(), batchSize);
        var totalGrabbed = 0;

        foreach (var chunk in chunks)
        {
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
        }

        _logger.LogInformation("Finished importing to {procedure} in {time}", procedureName, stopwatch.Elapsed);
    }



    // Bulk upsert for agencies
    public async Task UpsertAgenciesAsync(IEnumerable<Agency> agencies)
    {
        const string procedureName = "public.upsert_agencies";
        const string tvpTypeName = "public.agencies_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, agencies, 100);
    }

    // Bulk upsert for routes
    public async Task UpsertRoutesAsync(IEnumerable<Route> routes)
    {
        const string procedureName = "public.upsert_routes";
        const string tvpTypeName = "public.routes_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(routes), 5000);
    }

    public async Task UpsertCalendarsAsync(IEnumerable<Calendar> calenders)
    {
        const string procedureName = "public.upsert_calenders";
        const string tvpTypeName = "public.calenders_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, calenders, 5000);
    }

    // Bulk upsert for stops
    public async Task UpsertStopsAsync(IEnumerable<Stop> stops)
    {
        const string procedureName = "public.upsert_stops";
        const string tvpTypeName = "public.stops_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(stops), 1000);
    }

    // Bulk upsert for trips
    public async Task UpsertTripsAsync(IEnumerable<Trip> trips)
    {
        const string procedureName = "public.upsert_trips";
        const string tvpTypeName = "public.trips_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(trips), 1000);
    }

    // Bulk upsert for calendar dates
    public async Task UpsertCalendarDatesAsync(IEnumerable<CalendarDate> calendarDates)
    {
        const string procedureName = "public.upsert_calendar_dates";
        const string tvpTypeName = "public.calendar_dates_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(calendarDates), 5000);
    }

    // Bulk upsert for frequencies
    public async Task UpsertFrequenciesAsync(IEnumerable<Frequency> frequencies)
    {
        const string procedureName = "public.upsert_frequencies";
        const string tvpTypeName = "public.frequencies_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, frequencies, 5000);
    }

    // Bulk upsert for stop times
    public async Task UpsertStopTimesAsync(IEnumerable<StopTime> stopTimes)
    {
        const string procedureName = "public.upsert_stop_times";
        const string tvpTypeName = "public.stop_times_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(stopTimes), 1000);
    }

    // Bulk upsert for shapes
    public async Task UpsertShapesAsync(IEnumerable<Shape> shapes)
    {
        const string procedureName = "public.upsert_shapes";
        const string tvpTypeName = "public.shapes_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, ToPsql(shapes), 1000);
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
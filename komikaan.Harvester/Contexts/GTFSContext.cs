using GTFS.Entities;
using GTFS.Entities.Enumerations;
using Npgsql;
using Route = GTFS.Entities.Route;
using System.Diagnostics;
using Dapper;
using System.Data;

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
        builder.MapComposite<Trip>("public.trips_type");
        builder.MapComposite<Agency>("public.agencies_type");
        builder.MapComposite<Route>("public.routes_type");
        builder.MapComposite<Stop>("public.stops_type");
        builder.MapComposite<Calendar>("public.calenders_type");
        builder.MapComposite<CalendarDate>("public.calendar_dates_type");
        builder.MapComposite<Frequency>("public.frequencies_type");
        builder.MapComposite<StopTime>("public.stop_times_type");
        builder.MapComposite<Shape>("public.shapes_type");

        // Register the type handler
        SqlMapper.AddTypeHandler(new EnumTypeHandler<RouteTypeExtended>());
        //builder.MapEnum<RouteTypeExtended>("integer");

        // Build the NpgsqlDataSource
        _dataSource = builder.Build();
    }


    private async Task UpsertEntityAsync<T>(string procedureName, string tvpTypeName, IEnumerable<T> entities)
    {
        var stopwatch = Stopwatch.StartNew();
        var size = 500;
        _logger.LogInformation("Importing to {procedure}", procedureName);
        var chunks = entities.Chunk(size);
        _logger.LogInformation("Split into {amount} of chunks of {size}", chunks.Count(), size);
        var totalGrabbed = 0;

        foreach (var chunk in chunks)
        {
            totalGrabbed += 1;
            _logger.LogInformation("Working on {grab}/{total}", totalGrabbed, chunks.Count());

            // Preprocess entities to convert enums to integers
            var processedChunk = chunk.Select(e => ConvertEnumsToIntegers(e)).ToArray();

            using (var connection = _dataSource.CreateConnection())
            {
                var command = new NpgsqlCommand($"CALL {procedureName}(@items)", connection);
                await connection.OpenAsync();

                // Pass the array of objects directly, Npgsql will handle serialization
                var parameter = command.Parameters.AddWithValue("@items", processedChunk);

                // Specify the custom composite type (e.g., public.routes_type) for the array
                parameter.DataTypeName = tvpTypeName + "[]";  // Specify the array of custom composite type

                await command.ExecuteNonQueryAsync();
            }
        }

        _logger.LogInformation("Finished importing to {procedure} in {time}", procedureName, stopwatch.Elapsed);
    }

    // Helper method to recursively convert enums in an object to integers
    private static T ConvertEnumsToIntegers<T>(T entity)
    {
        if (entity == null)
            return default;

        var type = entity.GetType();
        var properties = type.GetProperties();

        // Create a copy of the object to modify
        var result = Activator.CreateInstance(type);

        foreach (var property in properties)
        {
            var value = property.GetValue(entity);

            if (value != null)
            {
                if (property.PropertyType.IsEnum)
                {
                    // Convert enum to its integer value
                    property.SetValue(result, Convert.ToInt32(value));
                }
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    // If the property is a class (but not a string), recurse
                    var convertedValue = ConvertEnumsToIntegers(value);
                    property.SetValue(result, convertedValue);
                }
                else
                {
                    // For all other types (non-enum and non-class), just copy the value
                    property.SetValue(result, value);
                }
            }
        }

        return (T)result;
    }

    // Bulk upsert for agencies
    public async Task UpsertAgenciesAsync(IEnumerable<Agency> agencies)
    {
        const string procedureName = "public.upsert_agencies";
        const string tvpTypeName = "public.agencies_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, agencies);
    }

    // Bulk upsert for routes
    public async Task UpsertRoutesAsync(IEnumerable<Route> routes)
    {
        const string procedureName = "public.upsert_routes";
        const string tvpTypeName = "public.routes_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, routes);
    }

    public async Task UpsertCalendarsAsync(IEnumerable<Calendar> calenders)
    {
        const string procedureName = "public.upsert_calenders";
        const string tvpTypeName = "public.calenders_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, calenders);
    }

    // Bulk upsert for stops
    public async Task UpsertStopsAsync(IEnumerable<Stop> stops)
    {
        const string procedureName = "public.upsert_stops";
        const string tvpTypeName = "public.stops_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, stops);
    }

    // Bulk upsert for trips
    public async Task UpsertTripsAsync(IEnumerable<Trip> trips)
    {
        const string procedureName = "public.upsert_trips";
        const string tvpTypeName = "public.trips_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, trips);
    }

    // Bulk upsert for calendar dates
    public async Task UpsertCalendarDatesAsync(IEnumerable<CalendarDate> calendarDates)
    {
        const string procedureName = "public.upsert_calendar_dates";
        const string tvpTypeName = "public.calendar_dates_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, calendarDates);
    }

    // Bulk upsert for frequencies
    public async Task UpsertFrequenciesAsync(IEnumerable<Frequency> frequencies)
    {
        const string procedureName = "public.upsert_frequencies";
        const string tvpTypeName = "public.frequencies_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, frequencies);
    }

    // Bulk upsert for stop times
    public async Task UpsertStopTimesAsync(IEnumerable<StopTime> stopTimes)
    {
        const string procedureName = "public.upsert_stop_times";
        const string tvpTypeName = "public.stop_times_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, stopTimes);
    }

    // Bulk upsert for shapes
    public async Task UpsertShapesAsync(IEnumerable<Shape> shapes)
    {
        const string procedureName = "public.upsert_shapes";
        const string tvpTypeName = "public.shapes_type";
        await UpsertEntityAsync(procedureName, tvpTypeName, shapes);
    }

}

public class EnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : Enum
{
    public override T Parse(object value)
    {
        return (T)Enum.Parse(typeof(T), Convert.ToString(value));
    }

    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = (int)Enum.Parse(typeof(T), Convert.ToString(value));
        parameter.DbType = DbType.Int32;
    }
}
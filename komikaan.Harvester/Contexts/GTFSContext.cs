using GTFS.Entities;
using GTFS.Entities.Collections;
using GTFS.Entities.Enumerations;
using Npgsql;
using System.Xml.Linq;
using System;
using Route = GTFS.Entities.Route;
using Dapper;
using System.Data;

namespace komikaan.Harvester.Contexts;

internal class GTFSContext
{
    private readonly NpgsqlDataSource _dataSource;

    public GTFSContext(IConfiguration configuration)
    {
        // Get the connection string from configuration
        var connectionString = configuration.GetConnectionString("gtfs");

        // Build the NpgsqlDataSource
        var builder = new NpgsqlDataSourceBuilder(connectionString);

        // Map composite types for each entity (these are the custom types in your PostgreSQL DB)
        builder.MapComposite<Trip>("public.trips_type");
        builder.MapComposite<Agency>("public.agencies_type");
        builder.MapComposite<Route>("public.routes_type");
        builder.MapComposite<Stop>("public.stops_type");
        builder.MapComposite<Calendar>("public.calenders_type"); // Mapping for Calendar
        builder.MapComposite<CalendarDate>("public.calendar_dates_type");
        builder.MapComposite<Frequency>("public.frequencies_type");
        builder.MapComposite<StopTime>("public.stop_times_type");
        builder.MapComposite<Shape>("public.shapes_type");

        // Build the NpgsqlDataSource
        _dataSource = builder.Build();
    }

    private async Task UpsertEntityAsync<T>(string procedureName, string tvpTypeName, IEnumerable<T> entities)
    {
        using (var connection = _dataSource.CreateConnection())
        {
            var command = new NpgsqlCommand($"CALL {procedureName}(@items)", connection);
            await connection.OpenAsync();

            command.Parameters.AddWithValue("@items", entities.ToArray());

            await command.ExecuteNonQueryAsync();
        }
    }

    // Helper method to get the property value dynamically
    private static object GetPropertyValue<T>(T entity, string propertyName)
    {
        return entity.GetType().GetProperty(propertyName)?.GetValue(entity);
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
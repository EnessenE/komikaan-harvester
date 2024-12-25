using GTFS.Entities;
using GTFS.Entities.Collections;
using GTFS.Entities.Enumerations;
using Npgsql;
using System.Xml.Linq;
using System;
using Route = GTFS.Entities.Route;

namespace komikaan.Harvester.Contexts;

internal class GTFSContext
{
    private readonly NpgsqlDataSource _dataSource;

    public GTFSContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("gtfs");
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        _dataSource = builder.Build();
    }

    internal async Task BulkMergeAgenciesAsync(IEnumerable<Agency> items)
    {
        var connection = _dataSource.CreateConnection();
        var transaction = connection.BeginTransaction();
        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.agencies(data_origin, id, name, url, timezone, language_code, phone, fare_url, email, internal_id, last_updated, import_id) VALUES (@data_origin, @id, @name, @url, @timezone, @language_code, @phone, @fare_url, @email, @internal_id, @last_updated, @import_id)",
            connection)
        {
            Transaction = transaction
        };

        foreach (var item in items)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@data_origin", item.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@id", item.Id);
            activePeriodCommand.Parameters.AddWithValue("@name", item.Name);
            activePeriodCommand.Parameters.AddWithValue("@url", item.URL);
            activePeriodCommand.Parameters.AddWithValue("@timezone", item.Timezone);
            activePeriodCommand.Parameters.AddWithValue("@language_code", item.LanguageCode);
            activePeriodCommand.Parameters.AddWithValue("@phone", item.Phone);
            activePeriodCommand.Parameters.AddWithValue("@fare_url", item.FareURL);
            activePeriodCommand.Parameters.AddWithValue("@email", item.Email);
            activePeriodCommand.Parameters.AddWithValue("@internal_id", item.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", item.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", item.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    internal async Task BulkMergeRoutesAsync(IEnumerable<Route> items)
    {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.routes(data_origin, id, agency_id, short_name, long_name, description, type, url, color, text_color, internal_id, last_updated, import_id) " +
            "VALUES (@data_origin, @id, @agency_id, @short_name, @long_name, @description, @type, @url, @color, @text_color, @internal_id, @last_updated, @import_id);",
            connection)
        {
            Transaction = transaction
        };

        foreach (var item in items)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@data_origin", item.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@id", item.Id);
            activePeriodCommand.Parameters.AddWithValue("@agency_id", item.AgencyId);
            activePeriodCommand.Parameters.AddWithValue("@short_name", item.ShortName);
            activePeriodCommand.Parameters.AddWithValue("@long_name", item.LongName);
            activePeriodCommand.Parameters.AddWithValue("@description", item.Description);
            activePeriodCommand.Parameters.AddWithValue("@type", item.Type);
            activePeriodCommand.Parameters.AddWithValue("@url", item.Url);
            activePeriodCommand.Parameters.AddWithValue("@color", item.Color);
            activePeriodCommand.Parameters.AddWithValue("@text_color", item.TextColor);
            activePeriodCommand.Parameters.AddWithValue("@internal_id", item.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", item.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", item.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    internal async Task BulkMergeStopsAsync(IUniqueEntityCollection<Stop> items)
    {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.stops(data_origin, id, code, name, description, latitude, longitude, geo_location, zone, url, location_type, parent_station, timezone, wheelchair_boarding, level_id, platform_code, stop_type, internal_id, last_updated, import_id) " +
            "VALUES (@data_origin, @id, @code, @name, @description, @latitude, @longitude, @geo_location, @zone, @url, @location_type, @parent_station, @timezone, @wheelchair_boarding, @level_id, @platform_code, @stop_type, @internal_id, @last_updated, @import_id);",
            connection)
        {
            Transaction = transaction
        };

        foreach (var item in items)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@data_origin", item.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@id", item.Id);
            activePeriodCommand.Parameters.AddWithValue("@code", item.Code);
            activePeriodCommand.Parameters.AddWithValue("@name", item.Name);
            activePeriodCommand.Parameters.AddWithValue("@description", item.Description);
            activePeriodCommand.Parameters.AddWithValue("@latitude", item.Latitude);
            activePeriodCommand.Parameters.AddWithValue("@longitude", item.Longitude);
            activePeriodCommand.Parameters.AddWithValue("@geo_location", item.GeoLocation); // Assuming GeoLocation is a type supported by Npgsql
            activePeriodCommand.Parameters.AddWithValue("@zone", item.Zone);
            activePeriodCommand.Parameters.AddWithValue("@url", item.Url);
            activePeriodCommand.Parameters.AddWithValue("@location_type", item.LocationType);
            activePeriodCommand.Parameters.AddWithValue("@parent_station", item.ParentStation);
            activePeriodCommand.Parameters.AddWithValue("@timezone", item.Timezone);
            activePeriodCommand.Parameters.AddWithValue("@wheelchair_boarding", item.WheelchairBoarding);
            activePeriodCommand.Parameters.AddWithValue("@level_id", item.LevelId);
            activePeriodCommand.Parameters.AddWithValue("@platform_code", item.PlatformCode);
            activePeriodCommand.Parameters.AddWithValue("@stop_type", item.StopType);
            activePeriodCommand.Parameters.AddWithValue("@internal_id", item.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", item.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", item.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    internal async Task BulkMergeTripsAsync(IUniqueEntityCollection<Trip> items)
    {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.trips(data_origin, id, route_id, service_id, headsign, short_name, direction, block_id, shape_id, accessibility_type, internal_id, last_updated, import_id) " +
            "VALUES (@data_origin, @id, @route_id, @service_id, @headsign, @short_name, @direction, @block_id, @shape_id, @accessibility_type, @internal_id, @last_updated, @import_id);",
            connection)
        {
            Transaction = transaction
        };

        foreach (var item in items)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@data_origin", item.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@id", item.Id);
            activePeriodCommand.Parameters.AddWithValue("@route_id", item.RouteId);
            activePeriodCommand.Parameters.AddWithValue("@service_id", item.ServiceId);
            activePeriodCommand.Parameters.AddWithValue("@headsign", item.Headsign);
            activePeriodCommand.Parameters.AddWithValue("@short_name", item.ShortName);
            activePeriodCommand.Parameters.AddWithValue("@direction", item.Direction);
            activePeriodCommand.Parameters.AddWithValue("@block_id", item.BlockId);
            activePeriodCommand.Parameters.AddWithValue("@shape_id", item.ShapeId);
            activePeriodCommand.Parameters.AddWithValue("@accessibility_type", item.AccessibilityType);
            activePeriodCommand.Parameters.AddWithValue("@internal_id", item.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", item.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", item.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    internal async Task BulkMergeCalendarsAsync(IEntityCollection<Calendar> calendars)
    {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.calendars(data_origin, service_id, mask, monday, tuesday, wednesday, thursday, friday, saturday, sunday, start_date, end_date, internal_id, last_updated, import_id) " +
            "VALUES (@data_origin, @service_id, @mask, @monday, @tuesday, @wednesday, @thursday, @friday, @saturday, @sunday, @start_date, @end_date, @internal_id, @last_updated, @import_id);",
            connection)
        {
            Transaction = transaction
        };

        foreach (var calendar in calendars)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@data_origin", calendar.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@service_id", calendar.ServiceId);
            activePeriodCommand.Parameters.AddWithValue("@mask", calendar.Mask);
            activePeriodCommand.Parameters.AddWithValue("@monday", calendar.Monday);
            activePeriodCommand.Parameters.AddWithValue("@tuesday", calendar.Tuesday);
            activePeriodCommand.Parameters.AddWithValue("@wednesday", calendar.Wednesday);
            activePeriodCommand.Parameters.AddWithValue("@thursday", calendar.Thursday);
            activePeriodCommand.Parameters.AddWithValue("@friday", calendar.Friday);
            activePeriodCommand.Parameters.AddWithValue("@saturday", calendar.Saturday);
            activePeriodCommand.Parameters.AddWithValue("@sunday", calendar.Sunday);
            activePeriodCommand.Parameters.AddWithValue("@start_date", calendar.StartDate);
            activePeriodCommand.Parameters.AddWithValue("@end_date", calendar.EndDate);
            activePeriodCommand.Parameters.AddWithValue("@internal_id", calendar.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", calendar.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", calendar.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    internal async Task BulkMergeCalendarDatesAsync(IEntityCollection<CalendarDate> calendarDates)
    {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.calendar_dates(data_origin, service_id, date, exception_type, internal_id, last_updated, import_id) " +
            "VALUES (@data_origin, @service_id, @date, @exception_type, @internal_id, @last_updated, @import_id);",
            connection)
        {
            Transaction = transaction
        };

        foreach (var calendarDate in calendarDates)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@data_origin", calendarDate.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@service_id", calendarDate.ServiceId);
            activePeriodCommand.Parameters.AddWithValue("@date", calendarDate.Date);
            activePeriodCommand.Parameters.AddWithValue("@exception_type", calendarDate.ExceptionType);
            activePeriodCommand.Parameters.AddWithValue("@internal_id", calendarDate.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", calendarDate.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", calendarDate.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
    internal async Task BulkMergeFrequenciesAsync(IEntityCollection<Frequency> frequencies)
    {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.frequencies(data_origin, trip_id, start_time, end_time, headway_secs, exact_times, internal_id, last_updated, import_id) " +
            "VALUES (@data_origin, @trip_id, @start_time, @end_time, @headway_secs, @exact_times, @internal_id, @last_updated, @import_id);",
            connection)
        {
            Transaction = transaction
        };

        foreach (var frequency in frequencies)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@data_origin", frequency.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@trip_id", frequency.TripId);
            activePeriodCommand.Parameters.AddWithValue("@start_time", frequency.StartTime);
            activePeriodCommand.Parameters.AddWithValue("@end_time", frequency.EndTime);
            activePeriodCommand.Parameters.AddWithValue("@headway_secs", frequency.HeadwaySecs);
            activePeriodCommand.Parameters.AddWithValue("@exact_times", frequency.ExactTimes);
            activePeriodCommand.Parameters.AddWithValue("@internal_id", frequency.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", frequency.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", frequency.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
    internal async Task BulkMergeStopTimesAsync(IStopTimeCollection stopTimes)
    {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.stop_times(data_origin, trip_id, stop_id, stop_sequence, arrival_time, departure_time, stop_headsign, pickup_type, drop_off_type, shape_dist_travelled, timepoint_type, internal_id, last_updated, import_id) " +
            "VALUES (@data_origin, @trip_id, @stop_id, @stop_sequence, @arrival_time, @departure_time, @stop_headsign, @pickup_type, @drop_off_type, @shape_dist_travelled, @timepoint_type, @internal_id, @last_updated, @import_id);",
            connection)
        {
            Transaction = transaction
        };

        foreach (var stopTime in stopTimes)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@data_origin", stopTime.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@trip_id", stopTime.TripId);
            activePeriodCommand.Parameters.AddWithValue("@stop_id", stopTime.StopId);
            activePeriodCommand.Parameters.AddWithValue("@stop_sequence", stopTime.StopSequence);
            activePeriodCommand.Parameters.AddWithValue("@arrival_time", stopTime.ArrivalTime);
            activePeriodCommand.Parameters.AddWithValue("@departure_time", stopTime.DepartureTime);
            activePeriodCommand.Parameters.AddWithValue("@stop_headsign", stopTime.StopHeadsign);
            activePeriodCommand.Parameters.AddWithValue("@pickup_type", stopTime.PickupType);
            activePeriodCommand.Parameters.AddWithValue("@drop_off_type", stopTime.DropOffType);
            activePeriodCommand.Parameters.AddWithValue("@shape_dist_travelled", stopTime.ShapeDistTravelled);
            activePeriodCommand.Parameters.AddWithValue("@timepoint_type", stopTime.TimepointType);
            activePeriodCommand.Parameters.AddWithValue("@internal_id", stopTime.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", stopTime.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", stopTime.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
    internal async Task BulkMergeShapesAsync(IEntityCollection<Shape> shapes)
    {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var activePeriodCommand = new NpgsqlCommand(
            "INSERT INTO public.shapes(internal_id, data_origin, id, sequence, latitude, longitude, geo_location, distance_travelled, last_updated, import_id) " +
            "VALUES (@internal_id, @data_origin, @id, @sequence, @latitude, @longitude, @geo_location, @distance_travelled, @last_updated, @import_id);",
            connection)
        {
            Transaction = transaction
        };

        foreach (var shape in shapes)
        {
            activePeriodCommand.Parameters.Clear();
            activePeriodCommand.Parameters.AddWithValue("@internal_id", shape.InternalId);
            activePeriodCommand.Parameters.AddWithValue("@data_origin", shape.DataOrigin);
            activePeriodCommand.Parameters.AddWithValue("@id", shape.Id);
            activePeriodCommand.Parameters.AddWithValue("@sequence", shape.Sequence);
            activePeriodCommand.Parameters.AddWithValue("@latitude", shape.Latitude);
            activePeriodCommand.Parameters.AddWithValue("@longitude", shape.Longitude);
            activePeriodCommand.Parameters.AddWithValue("@geo_location", shape.GeoLocation);
            activePeriodCommand.Parameters.AddWithValue("@distance_travelled", shape.DistanceTravelled);
            activePeriodCommand.Parameters.AddWithValue("@last_updated", shape.LastUpdated);
            activePeriodCommand.Parameters.AddWithValue("@import_id", shape.ImportId);

            await activePeriodCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

}
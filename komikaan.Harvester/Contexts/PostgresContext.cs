using GTFS;
using GTFS.Entities;
using komikaan.Harvester.Interfaces;
using Microsoft.Extensions.Options;
using Z.BulkOperations;

namespace komikaan.Harvester.Contexts;

internal class PostgresContext : IDataContext
{
    private readonly ILogger<PostgresContext> _logger;
    private readonly GTFSContext _gtfsContext;

    public PostgresContext(ILogger<PostgresContext> logger, GTFSContext gtfsContext)
    {
        _logger = logger;
        _gtfsContext = gtfsContext;
    }

    public async Task ImportAsync(GTFSFeed feed)
    {
        _gtfsContext.Agencies.BulkMerge(feed.Agencies, operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.Id, c.DataOrigin };

        });
        _gtfsContext.Routes.BulkMerge(feed.Routes, operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.Id, c.DataOrigin };

        });
        _gtfsContext.Trips.BulkMerge(feed.Trips.ToList(), operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.Id, c.DataOrigin };

        });
        _gtfsContext.Stops.BulkMerge(feed.Stops, operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.Id, c.DataOrigin };

        });
        _gtfsContext.Calendars.BulkMerge(feed.Calendars, operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.ServiceId, c.DataOrigin };

        });
        _gtfsContext.CalendarDates.BulkMerge(feed.CalendarDates, operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.ServiceId, c.Date, c.DataOrigin };

        });
        _gtfsContext.Frequencies.BulkMerge(feed.Frequencies, operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.TripId, c.DataOrigin, c.StartTime, c.EndTime };

        });
        _gtfsContext.StopTimes.BulkMerge(feed.StopTimes, operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.TripId, c.StopId, c.DataOrigin };

        });
        _gtfsContext.Shapes.BulkMerge(feed.Shapes.ToList(), operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.ColumnPrimaryKeyExpression = c => new { c.Id, c.DataOrigin, c.Sequence };
        });

        await _gtfsContext.SaveChangesAsync();
        _logger.LogInformation("Done with import.");
    }
}
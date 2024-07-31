using Dapper;
using GTFS;
using komikaan.Common.Models;
using komikaan.Harvester.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace komikaan.Harvester.Contexts;

internal class PostgresContext : IDataContext
{
    private readonly ILogger<PostgresContext> _logger;
    private readonly GTFSContext _gtfsContext;

    private readonly string _connectionString;

    public PostgresContext(ILogger<PostgresContext> logger, GTFSContext gtfsContext, IConfiguration configuration)
    {
        _logger = logger;
        _gtfsContext = gtfsContext;
        _connectionString = configuration.GetConnectionString("HarvestingTarget") ?? throw new InvalidOperationException("A GTFS postgres database connection should be defined!");

    }

    public async Task MarkDownload(SupplierConfiguration config)
    {
        using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

        await dbConnection.ExecuteAsync(
         @"CALL public.update_supplier_for_download(@target, @last_update, @pending)",
        new
        {
            target = config.Name,
            last_update = config.LastUpdated,
            pending = config.DownloadPending
        },
             commandType: CommandType.Text
         );
    }

    public async Task DeleteOldDataAsync(SupplierConfiguration config)
    {
        await _gtfsContext.Routes.BulkDeleteAsync(_gtfsContext.Routes.Where(item => item.DataOrigin.Equals(config.Name, StringComparison.InvariantCultureIgnoreCase) && item.ImportId == config.ImportId));
        await _gtfsContext.Trips.BulkDeleteAsync(_gtfsContext.Trips.Where(item => item.DataOrigin.Equals(config.Name, StringComparison.InvariantCultureIgnoreCase) && item.ImportId == config.ImportId));
        await _gtfsContext.Stops.BulkDeleteAsync(_gtfsContext.Stops.Where(item => item.DataOrigin.Equals(config.Name, StringComparison.InvariantCultureIgnoreCase) && item.ImportId == config.ImportId));
        await _gtfsContext.Calendars.BulkDeleteAsync(_gtfsContext.Calendars.Where(item => item.DataOrigin.Equals(config.Name, StringComparison.InvariantCultureIgnoreCase) && item.ImportId == config.ImportId));
        await _gtfsContext.CalendarDates.BulkDeleteAsync(_gtfsContext.CalendarDates.Where(item => item.DataOrigin.Equals(config.Name, StringComparison.InvariantCultureIgnoreCase) && item.ImportId == config.ImportId));
        await _gtfsContext.Frequencies.BulkDeleteAsync(_gtfsContext.Frequencies.Where(item => item.DataOrigin.Equals(config.Name, StringComparison.InvariantCultureIgnoreCase) && item.ImportId == config.ImportId));
        await _gtfsContext.StopTimes.BulkDeleteAsync(_gtfsContext.StopTimes.Where(item => item.DataOrigin.Equals(config.Name, StringComparison.InvariantCultureIgnoreCase) && item.ImportId == config.ImportId));
        await _gtfsContext.Shapes.BulkDeleteAsync(_gtfsContext.Shapes.Where(item => item.DataOrigin.Equals(config.Name, StringComparison.InvariantCultureIgnoreCase) && item.ImportId == config.ImportId));
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
        });
        _gtfsContext.StopTimes.BulkMerge(feed.StopTimes.ToList(), operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.BatchSize = 10000;

        });
        _gtfsContext.Shapes.BulkMerge(feed.Shapes.ToList(), operation =>
        {
            operation.InsertIfNotExists = true;
            operation.MergeKeepIdentity = true;
            operation.BatchSize = 10000;
            operation.ColumnPrimaryKeyExpression = c => new { c.Id, c.DataOrigin, c.Sequence };
        });

        await _gtfsContext.SaveChangesAsync();
        _logger.LogInformation("Done with import.");
    }

    private async Task BulkCopyAsync<T>(NpgsqlConnection connection, string tableName, IEnumerable<T> data)
    {
        using var writer = connection.BeginBinaryImport($"COPY {tableName} FROM STDIN (FORMAT BINARY)");
        foreach (var item in data)
        {
            writer.StartRow();
            // Write fields for item
            // Example: writer.Write(item.Property, NpgsqlTypes.NpgsqlDbType.Text);
        }
        await writer.CompleteAsync();
    }
}
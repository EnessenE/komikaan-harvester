using GTFS;
using komikaan.Harvester.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        await using var connection = new NpgsqlConnection(_gtfsContext.Database.GetConnectionString());
        await connection.OpenAsync();

        await BulkCopyAsync(connection, "agencies", feed.Agencies);
        await BulkCopyAsync(connection, "routes", feed.Routes);
        await BulkCopyAsync(connection, "trips", feed.Trips.ToList());
        await BulkCopyAsync(connection, "stops", feed.Stops);
        await BulkCopyAsync(connection, "calendars", feed.Calendars);
        await BulkCopyAsync(connection, "calendar_dates", feed.CalendarDates);
        await BulkCopyAsync(connection, "frequencies", feed.Frequencies);
        await BulkCopyAsync(connection, "stop_times", feed.StopTimes.ToList());
        await BulkCopyAsync(connection, "shapes", feed.Shapes.ToList());

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
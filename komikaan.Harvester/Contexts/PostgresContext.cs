using Dapper;
using komikaan.Common.Models;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Suppliers;
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
        _connectionString = configuration.GetConnectionString("gtfs") ?? throw new InvalidOperationException("A GTFS postgres database connection should be defined!");

    }

    public async Task MarkStartImportAsync(SupplierConfiguration config)
    {
        using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

        await dbConnection.ExecuteAsync(
         @"CALL public.harvester_mark_import_start(@data_origin)",
        new
        {
            data_origin = config.Name,
        },
             commandType: CommandType.Text
         );
    }

    public async Task UpdateImportStatusAsync(SupplierConfiguration config, string importStatus)
    {
        using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

        await dbConnection.ExecuteAsync(
         @"CALL public.harvester_update_status(@data_origin, @state)",
        new
        {
            data_origin = config.Name,
            state = importStatus,

        },
             commandType: CommandType.Text
         );
    }

    public async Task MarkDownloadAsync(SupplierConfiguration config, bool success)
    {
        using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

        await dbConnection.ExecuteAsync(
         @"CALL public.harvester_mark_supplier_failed(@target)",
        new
        {
            target = config.Name,
            last_update = config.LastUpdated,
            successfullyDownloaded = success
        },
             commandType: CommandType.Text
         );
    }

    public async Task CleanOldStopDataAsync(SupplierConfiguration config)
    {
        using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

        await dbConnection.ExecuteAsync(
         @"CALL public.clean_old_data()",
             commandType: CommandType.Text
         );
    }


    public async Task DeleteOldDataAsync(SupplierConfiguration config)
    {
        _logger.LogWarning("Moving to last import");
        using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);
        await dbConnection.ExecuteAsync(
          @"CALL public.move_to_new_import(@id, @dataorigin)",
             new
             {
                 id = config.ImportId,
                 dataorigin = config.Name,
             },
              commandType: CommandType.Text
          );
    }

    public async Task<List<SupplierTypeMapping>?> GetTypeMappingsAsync(SupplierConfiguration config)
    {
        using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

        var data = await dbConnection.QueryAsync<SupplierTypeMapping>(
        @"select * from get_mappings_from_supplier(@supplier)",
        new
            {
                supplier = config.Name
            },
            commandType: CommandType.Text
        );

        return data?.ToList();
    }


    public async Task ImportAsync(GTFSFeed feed)
    {

        //await _gtfsContext.UpsertAgenciesAsync(feed.Agencies);
        //await _gtfsContext.UpsertRoutesAsync(feed.Routes);
        //await _gtfsContext.UpsertTripsAsync(feed.Trips);
        //await _gtfsContext.UpsertStopsAsync(feed.Stops);
        //await _gtfsContext.UpsertCalendarsAsync(feed.Calendars);
        //await _gtfsContext.UpsertCalendarDatesAsync(feed.CalendarDates);
        //////await _gtfsContext.UpsertFrequenciesAsync(feed.Frequencies);
        //await _gtfsContext.UpsertStopTimesAsync(feed.StopTimes);
        //await _gtfsContext.UpsertShapesAsync(feed.Shapes);

        _logger.LogInformation("Done with import.");
    }
}
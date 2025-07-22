using System.Diagnostics;
using JNogueira.Discord.WebhookClient;
using komikaan.Common.Models;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Models;
using komikaan.Harvester.Suppliers;

namespace komikaan.Harvester.Managers
{

    /// <summary>
    /// Responsible for directing the import flow
    /// Calls different Suppliers and gets their data and passes it on towards our preferred shared data point
    /// </summary>
    public class HarvestingManager : IHostedService
    {
        private readonly IDataContext _dataContext;
        private readonly ILogger<HarvestingManager> _logger;
        private readonly IGardenerContext _gardenerContext;
        private readonly DiscordWebhookClient _discordWebHookClient;
        private readonly GenericGTFSSupplier _genericGTFSSupplier;

        public HarvestingManager(ILogger<HarvestingManager> logger, IDataContext dataContext, IGardenerContext gardenerContext, DiscordWebhookClient discordWebhookClient, GenericGTFSSupplier genericGTFSSupplier)
        {
            _logger = logger;
            _dataContext = dataContext;
            _gardenerContext = gardenerContext;
            _discordWebHookClient = discordWebhookClient;
            _genericGTFSSupplier = genericGTFSSupplier;

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting manager");

            await _gardenerContext.StartAsync(cancellationToken);
        }


        private async Task SendMessageAsync(SupplierConfiguration config, string body)
        {
            var message = new DiscordMessage("**Import progress for " + config.Name + "**\n" + body,
                username: Environment.MachineName,
                tts: false
            );
            await _discordWebHookClient.SendToDiscordAsync(message);

        }

        // A bit of a mess atm due to a GTFS dependency removal, will clean this up when its not summer
        public async Task Harvest(SupplierConfiguration config)
        {
            Directory.CreateDirectory(@"/app/");
            StaticImportData.CurrentDataOrigin = config.Name;
            StaticImportData.CurrentImportId = config.ImportId;

            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation("Starting import from {supplier}", config.Name);
                await SendMessageAsync(config, "Starting import, getting feed info");
                await _genericGTFSSupplier.RetrieveFeed(config);
                _logger.LogInformation("Finished importing data in {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                await SendMessageAsync(config, "Starting to delete old data");
                await _dataContext.DeleteOldDataAsync(config);
                _logger.LogInformation("Old data cleanup");
                await SendMessageAsync(config, "Cleaning old stops");
                await _dataContext.CleanOldStopDataAsync(config);
                await MarkAsFinished(config, true);
                _logger.LogInformation("Finished import in {time}", stopwatch.Elapsed.ToString("g"));
                await SendMessageAsync(config, "Finished import in " + stopwatch.Elapsed.ToString("g"));
            }
            catch (Exception error)
            {
                await SendMessageAsync(config, "Import failed! <@124928188647211009> \n " + error.Message);
                _logger.LogCritical("Failed import for {supplier}", config.Name);
                _logger.LogError(error, "Following error:");
                await MarkAsFinished(config, false);
                _logger.LogInformation("Marked as failed!");
                await _dataContext.UpdateImportStatusAsync(config, "import failed");
            }
            finally
            {
                File.Delete("\\app\\gtfs_file.zip");
                GC.Collect();
            }

        }

        private async Task MarkAsFinished(SupplierConfiguration config, bool success)
        {
            config.LastUpdated = DateTimeOffset.UtcNow;
            config.DownloadPending = false;
            await _dataContext.MarkDownloadAsync(config, success);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

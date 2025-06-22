using System.Diagnostics;
using JNogueira.Discord.WebhookClient;
using komikaan.Common.Models;
using komikaan.Harvester.Interfaces;
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

        public async Task Harvest(SupplierConfiguration config)
        {
            Directory.CreateDirectory(@"/app/");
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation("Starting import from {supplier}", config.Name);
                await SendMessageAsync(config, "Starting import, getting feed info");
                var feed = await _genericGTFSSupplier.RetrieveFeed(config);
                _logger.LogInformation("Finished retrieving data in {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);

                var mappings = await _dataContext.GetTypeMappingsAsync(config);
                _logger.LogInformation("Supplier has {x} mappings", mappings?.Count);
                _logger.LogInformation("Adjusting feed started {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                await SendMessageAsync(config, "Adjusting feed");
                await AdjustFeedAsync(feed, config, mappings);
                await SendMessageAsync(config, "Finished adjusting feed");
                _logger.LogInformation("Finished adjusting feed started {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                await SendMessageAsync(config, "Database import started!");
                await _dataContext.ImportAsync(feed);
                _logger.LogInformation("Finished importing data in {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                _logger.LogInformation("Notifying the gardeners for {name}", config.Name);

                await SendMessageAsync(config, "Finished, notifying gardeners");
                await NotifyAsync(feed);
                _logger.LogInformation("Notified the gardeners for {name}", config.Name);
                await SendMessageAsync(config, "Notified gardeners, starting to delete old data");
                await _dataContext.DeleteOldDataAsync(config);
                _logger.LogInformation("Old data cleanup");
                await SendMessageAsync(config, "Cleaning old stops");
                await _dataContext.CleanOldStopData(config);
                await MarkAsFinished(config, true);
                _logger.LogInformation("Finished import in {time}", stopwatch.Elapsed.ToString("g"));
                await SendMessageAsync(config, "Finished import in " + stopwatch.Elapsed.ToString("g"));
            }
            catch (Exception error)
            {
                await SendMessageAsync(config, "Import failed! <@124928188647211009>");
                _logger.LogCritical("Failed import for {supplier}", config.Name);
                _logger.LogError(error, "Following error:");
                await MarkAsFinished(config, false);
                _logger.LogInformation("Marked as failed!");
            }
            finally
            {
                File.Delete("\\app\\gtfs_file.zip");
            }

        }

        private async Task MarkAsFinished(SupplierConfiguration config, bool success)
        {
            config.LastUpdated = DateTimeOffset.UtcNow;
            config.DownloadPending = false;
            await _dataContext.MarkDownload(config, success);
        }

        private async Task AdjustFeedAsync(GTFSFeed feed, SupplierConfiguration config, List<SupplierTypeMapping> mappings)
        {
            var amountPerChunk = Math.Round((decimal)feed.Stops.Count() / 10, 0);
            var chunks = feed.Stops.ToList().Chunk((int)amountPerChunk);
            _logger.LogInformation("Split into {c} chunks of {amountPerChunk}", chunks.Count(), amountPerChunk);
            //var tasks = new List<Task>();
            //var totalUnknown = 0;
            //var chunk = 0;
            //foreach (var stops in chunks)
            //{
            //    chunk += 1;
            //    using (_logger.BeginScope(chunk))
            //    {
            //        var task = new Task(async () =>
            //        {
            //            var data = await DetectStopsType(feed, config, mappings, stops);
            //            Interlocked.Increment(ref totalUnknown);
            //        });
            //        task.Start();
            //        tasks.Add(task);
            //    }
            //}
            //Task.WaitAll(tasks.ToArray());
            //_logger.LogInformation("Total unknown stops: {total}", totalUnknown);
            await Task.CompletedTask;
        }

        //private async Task<int> DetectStopsType(GTFSFeed feed, SupplierConfiguration config, List<SupplierTypeMapping> mappings, IEnumerable<Stop> stops)
        //{
        //    var iteration = 0;
        //    int totalUnknown = 0;
        //    foreach (var stop in stops)
        //    {
        //        iteration += 1;
        //        if (iteration % 100 == 0 && iteration != 0)
        //        {
        //            _logger.LogInformation("{it}/{total} stop types", iteration, stops.Count());
        //        }

        //        try
        //        {
        //            if (feed.Stop_StopTimes.ContainsKey(stop.Id.ToLowerInvariant()))
        //            {
        //                var stopwatch = Stopwatch.StartNew();
        //                var relatedTimes = feed.Stop_StopTimes[stop.Id.ToLowerInvariant()].Take(1);
        //                // _logger.LogInformation("Got {x} times, {time}", relatedTimes.Count, stopwatch.ElapsedMilliseconds);
        //                var relatedTrips = feed.StopTime_Trips[relatedTimes.First().TripId.ToLowerInvariant()].Take(3);
        //                // _logger.LogInformation("Got {x} relatedTrips, {time}", relatedTrips.Count, stopwatch.ElapsedMilliseconds);
        //                var relatedRoutes = feed.Routes.Where(route => relatedTrips.Any(x => x.RouteId.Equals(route.Id))).Take(100);
        //                // _logger.LogInformation("Got {x} relatedRoutes, {time}", relatedRoutes.Count, stopwatch.ElapsedMilliseconds);
        //                var routeTypes = relatedRoutes.Select(route => route.Type).ToList();
        //                // _logger.LogInformation("Got {x} routeTypes, {time}", routeTypes.Count, stopwatch.ElapsedMilliseconds);
        //                var groupedTypes = routeTypes.GroupBy(x => ConvertStopType(x))
        //                         .ToDictionary(x => x.Key, y => y.Count());
        //                // _logger.LogInformation("Got {x} groupedTypes, {time}", groupedTypes.Count, stopwatch.ElapsedMilliseconds);


        //                if (groupedTypes.Count() == 1)
        //                {
        //                    var StopType = groupedTypes.First().Key;
        //                    stop.StopType = StopType;
        //                }
        //                else if (groupedTypes.Count() > 1)
        //                {
        //                    stop.StopType = StopType.Mixed;
        //                }
        //                else
        //                {
        //                    stop.StopType = StopType.Unknown;
        //                }


        //                if (mappings != null && mappings.Any())
        //                {
        //                    var overrideStopType = mappings.FirstOrDefault(mapping => mapping.ListedType == (int) stop.StopType);
        //                    if (overrideStopType != null)
        //                    {
        //                        stop.StopType = (StopType) overrideStopType.NewType;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                _logger.LogDebug("Forced {name} to unknown", stop.Name);
        //                Interlocked.Increment(ref totalUnknown);
        //                stop.StopType = StopType.Unknown;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogDebug("Forced {name} to unknown", stop.Name);
        //            _logger.LogError(ex, "broken");

        //            Interlocked.Increment(ref totalUnknown);
        //            stop.StopType = StopType.Unknown;
        //        }
        //    }
        //    return totalUnknown;
        //}

        private Task NotifyAsync(GTFSFeed feed)
        {
            foreach (var stop in feed.Stops)
            {
                _gardenerContext.SendMessage(new GardernerNotification() { Stop = stop });

            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

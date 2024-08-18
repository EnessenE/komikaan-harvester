using System.Diagnostics;
using System.Diagnostics.Metrics;
using GTFS;
using GTFS.Entities;
using GTFS.Entities.Enumerations;
using JNogueira.Discord.Webhook.Client;
using komikaan.Common.Models;
using komikaan.Harvester.Contexts;
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
        private readonly GardenerContext _gardenerContext;
        private readonly DiscordWebhookClient _discordWebHookClient;
        private readonly GenericGTFSSupplier _genericGTFSSupplier;

        public HarvestingManager(ILogger<HarvestingManager> logger, IDataContext dataContext, GardenerContext gardenerContext, DiscordWebhookClient discordWebhookClient, GenericGTFSSupplier genericGTFSSupplier)
        {
            _logger = logger;
            _dataContext = dataContext;
            _gardenerContext = gardenerContext;
            _discordWebHookClient = discordWebhookClient;
            _genericGTFSSupplier = genericGTFSSupplier;
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
            await _discordWebHookClient.SendToDiscord(message);

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

                config.Mapping = await _dataContext.GetTypeMappingsAsync(config);
                _logger.LogInformation("Supplier has {x} mappings", config.Mapping?.Count);
                _logger.LogInformation("Adjusting feed started {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                await SendMessageAsync(config, "Adjusting feed");
                await AdjustFeedAsync(feed, config);
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
                await SendMessageAsync(config, "Import failed!");
                _logger.LogCritical("Failed import for {supplier}", config.Name);
                _logger.LogError(error, "Following error:");
            }
            finally
            {
                await MarkAsFinished(config, false);
                File.Delete("\\app\\gtfs_file.zip");
            }

        }

        private async Task MarkAsFinished(SupplierConfiguration config, bool success)
        {
            config.LastUpdated = DateTimeOffset.UtcNow;
            config.DownloadPending = false;
            await _dataContext.MarkDownload(config, success);
        }

        private async Task AdjustFeedAsync(GTFS.GTFSFeed feed, SupplierConfiguration config)
        {
            var amountPerChunk = Math.Round((decimal)feed.Stops.Count / 10, 0);
            var chunks = feed.Stops.ToList().Chunk((int)amountPerChunk);
            _logger.LogInformation("Split into {c} chunks of {amountPerChunk}", chunks.Count(), amountPerChunk);
            var tasks = new List<Task>();
            var totalUnknown = 0;
            foreach (var stops in chunks)
            {
                var task = new Task(async () =>
                {
                    var data = await DetectStopsType(feed, config, stops);
                    Interlocked.Increment(ref totalUnknown);
                });
                task.Start();
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            _logger.LogInformation("Total unknown stops: {total}", totalUnknown);
            await Task.CompletedTask;
        }

        private async Task<int> DetectStopsType(GTFS.GTFSFeed feed, SupplierConfiguration config, IEnumerable<Stop> stops)
        {
            var iteration = 0;
            int totalUnknown = 0;
            foreach (var stop in stops)
            {
                iteration = iteration + 1;
                if (iteration % 100 == 0 && iteration != 0)
                {
                    _logger.LogInformation("{it}/{total} stop types", iteration, stops.Count());
                }

                try
                {
                    if (feed.Stop_StopTimes.ContainsKey(stop.Id.ToLowerInvariant()))
                    {
                        var stopwatch = Stopwatch.StartNew();
                        var relatedTimes = feed.Stop_StopTimes[stop.Id.ToLowerInvariant()].Take(1);
                        // _logger.LogInformation("Got {x} times, {time}", relatedTimes.Count, stopwatch.ElapsedMilliseconds);
                        var relatedTrips = feed.StopTime_Trips[relatedTimes.First().TripId.ToLowerInvariant()];
                        // _logger.LogInformation("Got {x} relatedTrips, {time}", relatedTrips.Count, stopwatch.ElapsedMilliseconds);
                        var relatedRoutes = feed.Routes.Where(route => relatedTrips.Any(x => x.RouteId.Equals(route.Id))).Take(5);
                        // _logger.LogInformation("Got {x} relatedRoutes, {time}", relatedRoutes.Count, stopwatch.ElapsedMilliseconds);
                        var routeTypes = relatedRoutes.Select(route => route.Type).ToList();
                        // _logger.LogInformation("Got {x} routeTypes, {time}", routeTypes.Count, stopwatch.ElapsedMilliseconds);
                        var groupedTypes = routeTypes.GroupBy(x => ConvertStopType(x))
                                 .ToDictionary(x => x.Key, y => y.Count());
                        // _logger.LogInformation("Got {x} groupedTypes, {time}", groupedTypes.Count, stopwatch.ElapsedMilliseconds);



                        if (groupedTypes.Count() == 1)
                        {
                            var StopType = groupedTypes.First().Key;
                            stop.StopType = StopType;
                        }
                        else if (groupedTypes.Count() > 1)
                        {
                            stop.StopType = StopType.Mixed;
                        }
                        else
                        {
                            stop.StopType = StopType.Unknown;
                        }


                        if (config.Mapping != null && config.Mapping.Any())
                        {
                            var overrideStopType = config.Mapping.FirstOrDefault(mapping => mapping.ListedType == (int) stop.StopType);
                            if (overrideStopType != null)
                            {
                                stop.StopType = ConvertStopType(overrideStopType.NewType);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Forced {name} to unknown", stop.Name);
                        Interlocked.Increment(ref totalUnknown);
                        stop.StopType = StopType.Unknown;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Forced {name} to unknown", stop.Name);
                    _logger.LogError(ex, "broken");

                    Interlocked.Increment(ref totalUnknown);
                    stop.StopType = StopType.Unknown;
                }
            }
            return totalUnknown;
        }

        private Task NotifyAsync(GTFS.GTFSFeed feed)
        {
            foreach (var stop in feed.Stops)
            {
                _gardenerContext.SendMessage(new GardernerNotification() { Stop = stop });

            }
            return Task.CompletedTask;
        }

        public StopType ConvertStopType(RouteTypeExtended routeType)
        {
            switch (routeType)
            {
                case GTFS.Entities.Enumerations.RouteTypeExtended.RailwayService:
                    return StopType.Train;
                case RouteTypeExtended.RailReplacementBusService: return StopType.Train;
                case RouteTypeExtended.RailShuttleWithinComplex: return StopType.Train;
                case RouteTypeExtended.RailTaxiService: return StopType.Train;
                case RouteTypeExtended.AdditionalRailService: return StopType.Train;
                case RouteTypeExtended.InterRegionalRailService: return StopType.Train;
                case RouteTypeExtended.UrbanRailwayService: return StopType.Metro;
                case RouteTypeExtended.UrbanRailwayServiceDefault: return StopType.Metro;
                case RouteTypeExtended.HighSpeedRailService: return StopType.Train;
                case RouteTypeExtended.AllRailServices: return StopType.Train;
                case RouteTypeExtended.BusService: return StopType.Bus;
                case RouteTypeExtended.AllBusServices: return StopType.Bus;
                case RouteTypeExtended.DemandandResponseBusService: return StopType.Bus;
                case RouteTypeExtended.ExpressBusService: return StopType.Bus;
                case RouteTypeExtended.LocalBusService: return StopType.Bus;
                case RouteTypeExtended.NightBusService: return StopType.Bus;
                case RouteTypeExtended.MobilityBusforRegisteredDisabled: return StopType.Bus;
                case RouteTypeExtended.PostBusService: return StopType.Bus;
                case RouteTypeExtended.SightseeingBus: return StopType.Bus;
                case RouteTypeExtended.InternationalCoachService: return StopType.Coach;
                case RouteTypeExtended.TramService: return StopType.Tram;
                case RouteTypeExtended.AllTramServices: return StopType.Tram;
                case RouteTypeExtended.CityTramService: return StopType.Tram;
                case RouteTypeExtended.LocalTramService: return StopType.Tram;
                case RouteTypeExtended.RegionalTramService: return StopType.Tram;
                case RouteTypeExtended.ShuttleTramService: return StopType.Tram;
                case RouteTypeExtended.SightseeingTramService: return StopType.Tram;
                case RouteTypeExtended.WaterTaxiService: return StopType.Ferry;
                case RouteTypeExtended.WaterTransportService: return StopType.Ferry;
                case RouteTypeExtended.AllWaterTransportServices: return StopType.Ferry;
                case RouteTypeExtended.FerryService: return StopType.Ferry;
                case RouteTypeExtended.AirportLinkFerryService: return StopType.Ferry;
                case RouteTypeExtended.CarHighSpeedFerryService: return StopType.Ferry;
                case RouteTypeExtended.InternationalCarFerryService: return StopType.Ferry;
                case RouteTypeExtended.InternationalPassengerFerryService: return StopType.Ferry;
                case RouteTypeExtended.LocalCarFerryService: return StopType.Ferry;
                case RouteTypeExtended.NationalCarFerryService: return StopType.Ferry;
                case RouteTypeExtended.PassengerHighSpeedFerryService: return StopType.Ferry;
                case RouteTypeExtended.RegionalCarFerryService: return StopType.Ferry;
                case RouteTypeExtended.TrainFerryService: return StopType.Ferry;
                case RouteTypeExtended.ShuttleFerryService: return StopType.Ferry;
                case RouteTypeExtended.ScheduledFerryService: return StopType.Ferry;
                case RouteTypeExtended.RoadLinkFerryService: return StopType.Ferry;
                case RouteTypeExtended.RegionalPassengerFerryService: return StopType.Ferry;

                default:
                    _logger.LogInformation("Unknown type: {type}", routeType);
                    return StopType.Unknown;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

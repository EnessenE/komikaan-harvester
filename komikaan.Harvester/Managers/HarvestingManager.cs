using System.Diagnostics;
using GTFS.Entities;
using GTFS.Entities.Enumerations;
using komikaan.Harvester.Contexts;
using komikaan.Harvester.Interfaces;
using Microsoft.Extensions.Logging;

namespace komikaan.Harvester.Managers
{
    /// <summary>
    /// Responsible for directing the import flow
    /// Calls different Suppliers and gets their data and passes it on towards our preferred shared data point
    /// </summary>
    public class HarvestingManager : BackgroundService
    {
        private readonly IEnumerable<ISupplier> _suppliers;
        private readonly IDataContext _dataContext;
        private readonly ILogger<HarvestingManager> _logger;
        private readonly GardenerContext _gardenerContext;

        public HarvestingManager(IEnumerable<ISupplier> suppliers, ILogger<HarvestingManager> logger, IDataContext dataContext, GardenerContext gardenerContext)
        {
            _suppliers = suppliers;
            _logger = logger;
            _dataContext = dataContext;
            _gardenerContext = gardenerContext;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _gardenerContext.StartAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        public async Task Harvesting()
        {
            foreach (var supplier in _suppliers)
            {
                try
                {
                    var config = supplier.GetConfiguration();
                    var stopwatch = Stopwatch.StartNew();
                    _logger.LogInformation("Starting import from {supplier}", config.Name);

                    var feed = await supplier.GetFeedAsync();
                    _logger.LogInformation("Finished retrieving data in {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                    _logger.LogInformation("Adjusting feed started {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                    await AdjustFeedAsync(feed);
                    _logger.LogInformation("Finished adjusting feed started {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                    await _dataContext.ImportAsync(feed);
                    _logger.LogInformation("Finished importing data in {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                    _logger.LogInformation("Notifying the gardeners for {name}", config.Name);
                    await NotifyAsync(feed);
                    _logger.LogInformation("Notified the gardeners", config.Name);
                }
                catch (Exception error)
                {
                    _logger.LogCritical("Failed import for {supplier}", supplier);
                    _logger.LogError(error, "Following error:");
                }

            }
        }



        private async Task AdjustFeedAsync(GTFS.GTFSFeed feed)
        {
            var amountPerChunk = Math.Round((decimal)feed.Stops.Count / 30, 0);
            var chunks = feed.Stops.ToList().Chunk((int)amountPerChunk);
            _logger.LogInformation("Split into {c} chunks of {amountPerChunk}", chunks.Count(), amountPerChunk);
            var tasks = new List<Task>();
            foreach (var stops in chunks)
            {
                var task = new Task(async () =>
                {
                    await DetectStopsType(feed, stops);
                    _logger.LogInformation("FINISHED A TASK");
                });
                task.Start();
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            await Task.CompletedTask;
        }

        private async Task DetectStopsType(GTFS.GTFSFeed feed, IEnumerable<Stop> stops)
        {
            foreach (var stop in stops)
            {
                if (feed.Stop_StopTimes.ContainsKey(stop.Id))
                {
                    var stopwatch = Stopwatch.StartNew();
                    var relatedTimes = feed.Stop_StopTimes[stop.Id].Take(5).ToDictionary(x => x.TripId);
                   // _logger.LogInformation("Got {x} times, {time}", relatedTimes.Count, stopwatch.ElapsedMilliseconds);
                    var relatedTrips = feed.StopTime_Trips.Where(x => relatedTimes.ContainsKey(x.Key)).SelectMany(x => x.Value).ToList();
                   // _logger.LogInformation("Got {x} relatedTrips, {time}", relatedTrips.Count, stopwatch.ElapsedMilliseconds);
                    var relatedRoutes = feed.Routes.Where(route => relatedTrips.Any(relatedTrip => relatedTrip.RouteId.Equals(route.Id))).Take(5).ToList();
                   // _logger.LogInformation("Got {x} relatedRoutes, {time}", relatedRoutes.Count, stopwatch.ElapsedMilliseconds);
                    var routeTypes = relatedRoutes.Select(route => route.Type).ToList();
                   // _logger.LogInformation("Got {x} routeTypes, {time}", routeTypes.Count, stopwatch.ElapsedMilliseconds);
                    var groupedTypes = routeTypes.GroupBy(x => DetectStopType(x))
                             .ToDictionary(x => x.Key, y => y.Count());
                    _logger.LogInformation("Got {x} groupedTypes, {time}", groupedTypes.Count, stopwatch.ElapsedMilliseconds);

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
                }
                else
                {
                    _logger.LogInformation("Forced {name} to unknown", stop.Name);
                    stop.StopType = StopType.Unknown;
                }
            }
            await Task.CompletedTask;
        }

        private Task NotifyAsync(GTFS.GTFSFeed feed)
        {
            foreach (var stop in feed.Stops)
            {
                _gardenerContext.SendMessage(new GardernerNotification() { Stop = stop });

            }
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Harvesting();
        }

        public StopType DetectStopType(RouteTypeExtended routeType)
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
                case RouteTypeExtended.UrbanRailwayService: return StopType.Train;
                case RouteTypeExtended.UrbanRailwayServiceDefault: return StopType.Train;
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

                default:
                    _logger.LogInformation("Unknown type: {type}", routeType);
                    return StopType.Unknown;
            }
        }
    }
}

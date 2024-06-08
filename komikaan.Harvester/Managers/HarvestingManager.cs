using System.Diagnostics;
using GTFS.Entities.Enumerations;
using komikaan.Harvester.Contexts;
using komikaan.Harvester.Interfaces;

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
        }



        private Task AdjustFeedAsync(GTFS.GTFSFeed feed)
        {
            foreach (var stop in feed.Stops)
            {
                var relatedTimes = feed.StopTimes.Where(stopTime => stopTime.StopId.Equals(stop.Id)).Take(25);
                var relatedTrips = feed.Trips.Where(trip => relatedTimes.Any(relatedTime => relatedTime.TripId.Equals(trip.Id)));
                var relatedRoutes = feed.Routes.Where(route => relatedTrips.Any(relatedTrip => relatedTrip.RouteId.Equals(route.Id)));
                var routeTypes = relatedRoutes.Select(route => route.Type);
                var groupedTypes = routeTypes.GroupBy(DetectStopType)
                        .Select(group => new
                        {
                            StopType = group.Key,
                            Count = group.Count()
                        })
                        .OrderBy(x => x.StopType);

                if (groupedTypes.Count() > 1)
                {
                    var StopType = groupedTypes.First().StopType;
                    stop.StopType = StopType;
                }
                else
                {
                    stop.StopType = StopType.Mixed;
                }

            }
            return Task.CompletedTask;
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

                default:
                    return StopType.Unknown;
            }
        }
    }
}

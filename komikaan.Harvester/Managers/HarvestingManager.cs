using System.Diagnostics;
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
        public async Task Harvesting()
        {
            foreach (var supplier in _suppliers)
            {
                var config = supplier.GetConfiguration();
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation("Starting import from {supplier}", config.Name);

                var feed = await supplier.GetFeedAsync();
                _logger.LogInformation("Finished retrieving data in {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);

                await _dataContext.ImportAsync(feed);
                _logger.LogInformation("Finished importing data in {time} from {supplier}", stopwatch.Elapsed.ToString("g"), config.Name);
                _logger.LogInformation("Notifying the gardeners for {name}", config.Name);
                _gardenerContext.SendMessage(new GardernerNotification());
                _logger.LogInformation("Notified the gardeners", config.Name);
                
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Harvesting();
        }
    }
}

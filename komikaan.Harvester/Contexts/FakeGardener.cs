using komikaan.Harvester.Interfaces;

namespace komikaan.Harvester.Contexts
{
    public class FakeGardener : IGardenerContext
    {
        private ILogger<FakeGardener> _logger;

        public FakeGardener(ILogger<FakeGardener> logger)
        {
            _logger = logger;
        }

        public void SendMessage(object item)
        {
        }

        public Task StartAsync(CancellationToken token)
        {
            _logger.LogInformation("Fake gardener context started");
            return Task.CompletedTask;
        }
    }
}

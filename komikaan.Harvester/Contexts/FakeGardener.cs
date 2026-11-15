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

        public Task SendMessageAsync(object item)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken token)
        {
            _logger.LogInformation("Fake gardener context started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}

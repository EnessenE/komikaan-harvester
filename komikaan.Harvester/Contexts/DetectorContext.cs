using JNogueira.Discord.WebhookClient;
using komikaan.Common.Models;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Managers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace komikaan.Harvester.Contexts
{
    public class DetectorContext : IHostedService
    {
        private readonly ILogger<DetectorContext> _logger;
        private readonly HarvestingManager _harvestingManager;
        private readonly DiscordWebhookClient _discordWebHookClient;
        private IModel _channel;
        private IDataContext _dataContext;
        private readonly IConfiguration _configuration;

        public DetectorContext(ILogger<DetectorContext> logger, HarvestingManager harvestingManager, DiscordWebhookClient discordWebHookClient, IConfiguration configuration, IDataContext dataContext)
        {
            _logger = logger;
            _harvestingManager = harvestingManager;
            _discordWebHookClient = discordWebHookClient;
            _configuration = configuration;
            _dataContext = dataContext;
        }


        public async Task StartAsync(CancellationToken token)
        {
            if (_configuration.GetValue("localtesting", false))
            {
                _logger.LogWarning("Running in local test mode");
                await ProcessMessageAsync(new SupplierConfiguration
                {
                    RetrievalType = RetrievalType.LOCAL,
                    DataType = SupplierType.GTFS,
                    PollingRate = TimeSpan.Zero,
                    Name = "iledefrance",
                    Url = "C:\\Users\\Enes\\Desktop\\iledefrance.zip",
                    ETag = null,
                    ImportId = Guid.Parse("7be7fe00-cd6d-44df-84e7-23d7898b05a2"),
                    LatestSuccesfullImportId = Guid.Parse("7be7fe00-cd6d-44df-84e7-23d7898b05a2"),
                    LastUpdated = DateTimeOffset.UtcNow,
                    LastAttempt = null,
                    LastChecked = DateTimeOffset.UtcNow,
                    DownloadPending = false
                });
            }
            else
            {
                StartDetector();
            }
        }

        private void StartDetector()
        {
            _logger.LogInformation("Connecting to detector queue");
            var factory = new ConnectionFactory();

            factory.HostName = _configuration.GetValue<string>("RabbitMQHost")!;
            factory.UserName = _configuration.GetValue<string>("RabbitMQUsername")!;
            factory.Password = _configuration.GetValue<string>("RabbitMQPassword")!;
            factory.RequestedConnectionTimeout = TimeSpan.FromHours(3);
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _channel.BasicQos(0, 1, false);
            _channel.ContinuationTimeout = TimeSpan.FromHours(3);

            _channel.ExchangeDeclare("harvester-notifications", ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(queue: "harvesters",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.QueueBind("harvesters", "harvester-notifications", "harvester");

            var consumer = new EventingBasicConsumer(_channel);
            var importRunning = false;
            consumer.Received += async (model, ea) =>
            {
                if (!importRunning)
                {
                    importRunning = true;
                    _logger.LogInformation("Received a message!");
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var item = JsonSerializer.Deserialize<SupplierConfiguration>(message);
                    try
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                        await ProcessMessageAsync(item);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unknown error");
                        await SendMessageAsync(item, ex.Message);
                    }
                    importRunning = false;
                }
                else
                {
                    //This is a hack because I want to use RabbitMQ and that doesn't like long running tasks
                    //note: Why don't you just close the consumer?
                    //quick look over there!
                    _logger.LogInformation("Got an early message, waiting and NACKing it");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            _channel.BasicConsume(queue: "harvesters",
                                 autoAck: false,
                                 consumer: consumer, consumerTag: "harvester");
            _logger.LogInformation("Started, waiting for a new import");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(SupplierConfiguration item)
        {
            using (_logger.BeginScope("{name} - {import}", item.Name, item.ImportId))
            {
                _logger.LogInformation("Starting an import", item.Name);
                await _dataContext.MarkStartImportAsync(item);
                await _dataContext.UpdateImportStatusAsync(item, "Started");
                await _harvestingManager.Harvest(item);
            }
        }


        private async Task SendMessageAsync(SupplierConfiguration config, string body)
        {
            var message = new DiscordMessage("**FAILED IMPORT, CRITICAL FAILURE, DROPPING " + config.Name + "**\n" + body,
                username: Environment.MachineName,
                tts: false
            );
            try
            {
                await _discordWebHookClient.SendToDiscordAsync(message);
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Failed to send a message about a failure");
                await _discordWebHookClient.SendToDiscordAsync(new DiscordMessage("Unknown failure", Environment.MachineName));
            }

        }
    }
}

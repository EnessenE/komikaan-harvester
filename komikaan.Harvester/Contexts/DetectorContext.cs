using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using RabbitMQ.Client.Events;
using komikaan.Harvester.Managers;
using JNogueira.Discord.Webhook.Client;
using komikaan.Common.Models;

namespace komikaan.Harvester.Contexts
{
    public class DetectorContext : IHostedService
    {
        private readonly ILogger<DetectorContext> _logger;
        private readonly HarvestingManager _harvestingManager;
        private readonly DiscordWebhookClient _discordWebHookClient;
        private IModel _channel;

        public DetectorContext(ILogger<DetectorContext> logger, HarvestingManager harvestingManager, DiscordWebhookClient discordWebHookClient)
        {
            _logger = logger;
            _harvestingManager = harvestingManager;
            _discordWebHookClient = discordWebHookClient;
        }


        public Task StartAsync(CancellationToken token)
        {
            _logger.LogInformation("Connecting to detector queue");
            var factory = new ConnectionFactory { HostName = "localhost", };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _channel.BasicQos(0, 1, false);

            _channel.ExchangeDeclare("harvester-notifications", ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(queue: "harvesters",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.QueueBind("harvesters", "harvester-notifications", "harvester");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                _logger.LogInformation("Received a message!");
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var item = JsonSerializer.Deserialize<SupplierConfiguration>(message);
                try
                {
                    ProcessMessageAsync(item).GetAwaiter().GetResult();
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unknown error");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                    await SendMessageAsync(item, ex.Message);
                }
            };
            _channel.BasicConsume(queue: "harvesters",
                                 autoAck: false,
                                 consumer: consumer, consumerTag: "gardener");
            _logger.LogInformation("Started, waiting for a new import");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(SupplierConfiguration item)
        {
            _logger.LogInformation("Starting an import for {name}", item.Name);
            using (_logger.BeginScope(item.Name))
            {
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
                await _discordWebHookClient.SendToDiscord(message);
            }
            catch(Exception err)
            {
                _logger.LogError(err, "Failed to send a message about a failure");
                await _discordWebHookClient.SendToDiscord(new DiscordMessage("Unknown failure", Environment.MachineName));
            }

        }
    }
}

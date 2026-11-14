using komikaan.Harvester.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace komikaan.Harvester.Contexts
{
    public class GardenerContext : IGardenerContext
    {
        private IChannel _channel = null!;
        private readonly ILogger<GardenerContext> _logger;
        private readonly IConfiguration _configuration;

        public GardenerContext(ILogger<GardenerContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken token)
        {

            _logger.LogInformation("Connecting to gardener queue");
            var factory = new ConnectionFactory();

            factory.HostName = _configuration.GetValue<string>("RabbitMQHost")!;
            factory.UserName = _configuration.GetValue<string>("RabbitMQUsername")!;
            factory.Password = _configuration.GetValue<string>("RabbitMQPassword")!;
            var connection = await factory.CreateConnectionAsync();
            _channel = await connection.CreateChannelAsync();


            await _channel.ExchangeDeclarePassiveAsync("stop-notifications");
            await _channel.QueueDeclarePassiveAsync(queue: "gardeners");
            await _channel.QueueBindAsync("gardeners", "stop-notifications", "gardener");
            _logger.LogInformation("Connected to gardener queue");

        }

        public void SendMessage(object message)
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            var rawMessage = JsonSerializer.Serialize(message, options);
            var body = Encoding.UTF8.GetBytes(rawMessage);
            _channel.BasicPublishAsync(exchange: string.Empty,
                                 routingKey: "gardeners",
                                 mandatory: false,
                                 basicProperties: new BasicProperties(),
                                 body: body).GetAwaiter().GetResult();
        }
    }
}

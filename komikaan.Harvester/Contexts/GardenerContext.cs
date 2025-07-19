using komikaan.Harvester.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace komikaan.Harvester.Contexts
{
    public class GardenerContext : IGardenerContext
    {
        private IModel _channel;
        private readonly ILogger<GardenerContext> _logger;
        private readonly IConfiguration _configuration;

        public GardenerContext(ILogger<GardenerContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken token)
        {

            _logger.LogInformation("Connecting to gardener queue");
            var factory = new ConnectionFactory();

            factory.HostName = _configuration.GetValue<string>("RabbitMQHost")!;
            factory.UserName = _configuration.GetValue<string>("RabbitMQUsername")!;
            factory.Password = _configuration.GetValue<string>("RabbitMQPassword")!;
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();


            _channel.ExchangeDeclare("stop-notifications", "direct", true);
            _channel.QueueDeclare(queue: "gardeners", 
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.QueueBind("gardeners", "stop-notifications", "gardener");
            _logger.LogInformation("Connected to gardener queue");

            return Task.CompletedTask;
        }

        public void SendMessage(object message)
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            var rawMessage = JsonSerializer.Serialize(message, options);
            var body = Encoding.UTF8.GetBytes(rawMessage);
            _channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "gardeners",
                                 basicProperties: null,
                                 body: body);
        }
    }
}

using System.Text;
using RabbitMQ.Client;
using System.Text.Json;

namespace komikaan.Harvester.Contexts
{
    public class GardenerContext
    {
        private IModel _channel;
        private ILogger<GardenerContext> _logger;

        public GardenerContext(ILogger<GardenerContext> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken token)
        {

            _logger.LogInformation("Connecting to gardener queue");
            var factory = new ConnectionFactory { HostName = "localhost" };
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

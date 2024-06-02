using Microsoft.AspNetCore.Connections;
using System.Text;
using RabbitMQ.Client;
using System.Text.Json;

namespace komikaan.Harvester.Contexts
{
    public class GardenerContext
    {
        private IModel _channel;

        public Task StartAsync(CancellationToken token)
        {

            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            _channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            return Task.CompletedTask;
        }

        public void SendMessage(object message)
        {
            var rawMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(rawMessage);
            _channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "hello",
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine($" [x] Sent {rawMessage}");

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}

using komikaan.Harvester.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace komikaan.Harvester.Contexts
{
    public class GardenerContext : IGardenerContext
    {
        private IChannel _channel = null!;
        private IConnection? _connection;
        private readonly ILogger<GardenerContext> _logger;
        private readonly IConfiguration _configuration;

        public GardenerContext(ILogger<GardenerContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken token)
        {
            var attempt = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    attempt++;
                    _logger.LogInformation("Connecting to gardener queue (attempt {attempt})", attempt);
                    var factory = RabbitMQConnectionFactory.Build(_configuration);
                    var connection = await factory.CreateConnectionAsync(token);
                    connection.ConnectionShutdownAsync += (_, ea) =>
                    {
                        _logger.LogWarning("Gardener RabbitMQ connection shut down: {reason}", ea.ReplyText);
                        return Task.CompletedTask;
                    };
                    connection.RecoverySucceededAsync += (_, _) =>
                    {
                        _logger.LogInformation("Gardener RabbitMQ connection recovered");
                        return Task.CompletedTask;
                    };
                    _connection = connection;
                    _channel = await connection.CreateChannelAsync();

                    await _channel.ExchangeDeclarePassiveAsync("stop-notifications");
                    await _channel.QueueDeclarePassiveAsync(queue: "gardeners");
                    _logger.LogInformation("Connected to gardener queue");
                    return;
                }
                catch (Exception ex) when (!token.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Could not connect to gardener queue (attempt {attempt}), retrying in 10s", attempt);
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
            }
        }

        public async Task SendMessageAsync(object message)
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            var rawMessage = JsonSerializer.Serialize(message, options);
            var body = Encoding.UTF8.GetBytes(rawMessage);
            if (_channel is not { IsOpen: true })
            {
                // never fail the import over a notification; auto-recovery will restore the channel
                _logger.LogError("Gardener channel is closed, dropping notification for {message}", rawMessage);
                return;
            }
            await _channel.BasicPublishAsync(exchange: string.Empty,
                                 routingKey: "gardeners",
                                 mandatory: false,
                                 basicProperties: new BasicProperties(),
                                 body: body);
        }

        public async Task StopAsync(CancellationToken token)
        {
            try
            {
                if (_channel is { IsOpen: true })
                {
                    await _channel.CloseAsync(token);
                }
                if (_connection is { IsOpen: true })
                {
                    await _connection.CloseAsync(token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanly close the gardener RabbitMQ connection");
            }
        }
    }
}

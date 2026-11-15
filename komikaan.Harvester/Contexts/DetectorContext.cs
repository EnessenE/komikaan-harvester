using JNogueira.Discord.WebhookClient;
using komikaan.Common.Models;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Managers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Contexts
{
    public class DetectorContext : IHostedService
    {
        private readonly ILogger<DetectorContext> _logger;
        private readonly HarvestingManager _harvestingManager;
        private readonly DiscordWebhookClient _discordWebHookClient;
        private IChannel _channel = null!;
        private IConnection? _connection;
        private readonly IDataContext _dataContext;
        private readonly IConfiguration _configuration;
        private CancellationToken _stoppingToken;

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
            _stoppingToken = token;

            if (_configuration.GetValue("localtesting", false))
            {
                _logger.LogWarning("Running in local test mode");
                await ProcessMessageAsync(new ImportRequest
                {
                    RetrievalType = RetrievalType.LOCAL,
                    DataType = SupplierType.GTFS,
                    PollingRate = TimeSpan.Zero,
                    Name = "gob.es-metroval",
                    Url = "C:\\Users\\Enes\\Downloads\\20250927_220027_Metro_Valencia.zip",
                    ETag = null,
                    QueuedImportId = Guid.NewGuid(),
                    LastAttempt = null,
                    LastChecked = DateTimeOffset.UtcNow,
                    DownloadPending = false
                }, token);
            }
            else
            {
                await StartDetector();
            }
        }

        private async Task StartDetector()
        {
            // Retry until the broker and its topology are up; in k8s the operator may
            // create the exchange/queue after this pod starts, so a single attempt can fail.
            var attempt = 0;
            while (!_stoppingToken.IsCancellationRequested)
            {
                try
                {
                    attempt++;
                    _logger.LogInformation("Connecting to detector queue (attempt {attempt})", attempt);
                    var factory = RabbitMQConnectionFactory.Build(_configuration);
                    var connection = await factory.CreateConnectionAsync(_stoppingToken);
                    connection.ConnectionShutdownAsync += (_, ea) =>
                    {
                        _logger.LogWarning("Detector RabbitMQ connection shut down: {reason}", ea.ReplyText);
                        return Task.CompletedTask;
                    };
                    connection.RecoverySucceededAsync += (_, _) =>
                    {
                        _logger.LogInformation("Detector RabbitMQ connection recovered");
                        return Task.CompletedTask;
                    };
                    _connection = connection;
                    _channel = await connection.CreateChannelAsync();
                    await _channel.BasicQosAsync(0, 1, false);

                    await _channel.ExchangeDeclarePassiveAsync("harvester-notifications");
                    await _channel.QueueDeclarePassiveAsync(queue: "harvesters");
                    break;
                }
                catch (Exception ex) when (!_stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Could not connect to detector queue (attempt {attempt}), retrying in 10s", attempt);
                    await Task.Delay(TimeSpan.FromSeconds(10), _stoppingToken);
                }
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            var importRunning = false;
            consumer.ReceivedAsync += async (model, ea) =>
            {
                if (importRunning)
                {
                    //This is a hack because I want to use RabbitMQ and that doesn't like long running tasks
                    //note: Why don't you just close the consumer?
                    //quick look over there!
                    _logger.LogInformation("Got an early message, waiting and NACKing and requeuing it");
                    await Task.Delay(TimeSpan.FromMinutes(1), _stoppingToken);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    return;
                }

                importRunning = true;
                try
                {
                    _logger.LogInformation("Received a message!");
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    ImportRequest? item;
                    try
                    {
                        item = JsonSerializer.Deserialize<ImportRequest>(message);
                    }
                    catch (Exception ex)
                    {
                        // Poison message: ack (drop) it so it doesn't redeliver forever
                        _logger.LogError(ex, "Could not parse import request, dropping message");
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    if (item == null)
                    {
                        _logger.LogError("Received an empty import request, dropping message");
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    using (_logger.BeginScope("{name} - {import}", item.Name, item.QueuedImportId))
                    {
                        // Delayed imports are held by the broker (RabbitMQ 4.4 x-opt-delivery-time
                        // on the quorum queue, set by the publisher), so anything delivered here is due.
                        await StartImport(ea, item, _stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // Never leave importRunning stuck: requeue the message so it is retried later
                    _logger.LogError(ex, "Unexpected error while handling an import request, requeueing it");
                    try
                    {
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                    catch (Exception nackError)
                    {
                        _logger.LogError(nackError, "Failed to requeue the message");
                    }
                }
                finally
                {
                    importRunning = false;
                }
            };
            await _channel.BasicConsumeAsync(queue: "harvesters",
                                 autoAck: false,
                                 consumer: consumer, consumerTag: "harvester");
            _logger.LogInformation("Started, waiting for a new import");
        }

        private async Task StartImport(BasicDeliverEventArgs ea, ImportRequest item, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Import accepted");
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                await ProcessMessageAsync(item, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown error");
                await SendMessageAsync(item, ex.Message);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_channel != null)
                {
                    await _channel.CloseAsync(cancellationToken);
                }
                if (_connection != null)
                {
                    await _connection.CloseAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanly close the RabbitMQ connection");
            }
        }

        private async Task ProcessMessageAsync(ImportRequest item, CancellationToken cancellationToken)
        {
            await _dataContext.MarkStartImportAsync(item);
            await _dataContext.UpdateImportStatusAsync(item, "Started");
            await _harvestingManager.Harvest(item, cancellationToken);
           
        }


        private async Task SendMessageAsync(ImportRequest config, string body)
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
                try
                {
                    await _discordWebHookClient.SendToDiscordAsync(new DiscordMessage("Unknown failure", Environment.MachineName));
                }
                catch (Exception fallbackError)
                {
                    _logger.LogError(fallbackError, "Failed to send the fallback failure message");
                }
            }

        }
    }
}

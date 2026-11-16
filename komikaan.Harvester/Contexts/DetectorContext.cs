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
        private int _importRunning;

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
                    // Dispatch deliveries on 2 workers so the "early message" handler below can
                    // run (and nack-requeue) while another worker is busy with a long import.
                    _channel = await connection.CreateChannelAsync(new CreateChannelOptions(
                        publisherConfirmationsEnabled: false,
                        publisherConfirmationTrackingEnabled: false,
                        consumerDispatchConcurrency: 2));
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
            consumer.ReceivedAsync += async (model, ea) =>
            {
                if (Interlocked.CompareExchange(ref _importRunning, 1, 0) != 0)
                {
                    // An import is already running on this pod. With prefetch=1 the broker delivered
                    // this next message right after the current one was acked; left alone it would sit
                    // unacknowledged for the whole import and trip consumer_timeout (30 min). Nack-requeue
                    // it every 20s instead: it stays in RabbitMQ and the broker's ack timer resets on every
                    // redelivery. Once the import finishes, the message is redelivered and processed for real.
                    _logger.LogInformation("Got an early message, NACKing and requeuing it every 20s until the current import is done");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(20), _stoppingToken);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to NACK/requeue an early message");
                    }
                    return;
                }

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
                    // Never leave _importRunning stuck: requeue the message so it is retried later
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
                    Interlocked.Exchange(ref _importRunning, 0);
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

using RabbitMQ.Client;

namespace komikaan.Harvester.Contexts
{
    /// <summary>
    /// Builds RabbitMQ connection factories with sane resilience settings.
    /// Without explicit heartbeats/recovery the client can end up in a state where the
    /// connection is dead but nothing fails or logs, so consumers silently stop receiving.
    /// </summary>
    public static class RabbitMQConnectionFactory
    {
        public static ConnectionFactory Build(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration.GetValue<string>("RabbitMQHost")!,
                UserName = configuration.GetValue<string>("RabbitMQUsername")!,
                Password = configuration.GetValue<string>("RabbitMQPassword")!,
                // fail fast instead of hanging for hours on a half-dead connection
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                ContinuationTimeout = TimeSpan.FromSeconds(30),
                // detect dead connections quickly and recover automatically
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
            return factory;
        }
    }
}

using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.ExchangePolling.Core.Settings.JobSettings
{
    /// <summary>
    /// RabbitMQ Connection Params. ExchangeType is always fanout. RoutingKey is always null.
    /// </summary>
    public class RabbitMQConnectionParams
    {
        [Optional]
        public string ConnectionString { get; set; }

        [Optional]
        public string ExchangeName { get; set; }

        [Optional]
        public string ExchangeType => "fanout";

        [Optional]
        public string QueueName { get; set; }

        [Optional]
        public string RoutingKey => null;
    }
}

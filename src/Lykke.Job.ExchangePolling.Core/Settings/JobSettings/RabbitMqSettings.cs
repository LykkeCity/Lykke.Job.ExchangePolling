namespace Lykke.Job.ExchangePolling.Core.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        public RabbitMQConnectionParams ExchangeConnectorOrder { get; set; }
        
        public RabbitMQConnectionParams ExchangeConnectorQuotes { get; set; }
    }
}

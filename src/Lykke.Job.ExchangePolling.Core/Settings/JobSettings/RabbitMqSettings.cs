namespace Lykke.Job.ExchangePolling.Core.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        public RabbitMQConnectionParams ExchangeConnectorQuotes { get; set; }
        
        public RabbitMQConnectionParams ExchangeConnectorOrder { get; set; }
        
        public RabbitMQConnectionParams PositionControlOrder { get; set; }
        
        public RabbitMQConnectionParams NonStreamingOrder { get; set; }
    }
}

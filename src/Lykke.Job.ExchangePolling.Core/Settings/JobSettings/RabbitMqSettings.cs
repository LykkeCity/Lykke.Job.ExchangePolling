namespace Lykke.Job.LykkeJob.Core.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        public RabbitMQConnectionParams ExchangeConnectorOrder { get; set; }
        
        public RabbitMQConnectionParams ExchangeConnectorQuotesBTCUSD { get; set; }
    }
}

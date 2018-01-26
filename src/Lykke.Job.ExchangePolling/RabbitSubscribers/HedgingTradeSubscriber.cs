using Common.Log;

namespace Lykke.Job.ExchangePolling.RabbitSubscribers
{
    public class HedgingTradeSubscriber : RabbitMqSubscriber<Service.ExchangeConnector.Client.Models.ExecutionReport>
    {
        public HedgingTradeSubscriber(ILog log, string connectionString, string exchangeName, string queueName, bool isDurable)
            : base(log, connectionString, exchangeName, queueName, isDurable)
        {
            
        }
    }
}

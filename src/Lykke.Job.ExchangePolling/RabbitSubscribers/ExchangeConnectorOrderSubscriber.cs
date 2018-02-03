using Common.Log;

namespace Lykke.Job.ExchangePolling.RabbitSubscribers
{
    public class ExchangeConnectorOrderSubscriber : RabbitMqSubscriber<Service.ExchangeConnector.Client.Models.ExecutionReport>
    {
        public ExchangeConnectorOrderSubscriber(ILog log, string connectionString, string exchangeName, string queueName, bool isDurable)
            : base(log, connectionString, exchangeName, queueName, isDurable)
        {
            
        }
    }
}

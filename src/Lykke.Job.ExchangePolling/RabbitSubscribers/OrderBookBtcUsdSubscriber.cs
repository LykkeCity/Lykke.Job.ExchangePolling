using Common.Log;
using Lykke.Job.ExchangePolling.Contract;

namespace Lykke.Job.ExchangePolling.RabbitSubscribers
{
    public class OrderBookBtcUsdSubscriber : RabbitMqSubscriber<OrderBook>
    {
        public OrderBookBtcUsdSubscriber(ILog log, string connectionString, string exchangeName, string queueName, bool isDurable)
            : base(log, connectionString, exchangeName, queueName, isDurable)
        {
            
        }
    }
}

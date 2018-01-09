using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.ExchangePolling.RabbitSubscribers
{
    public class RabbitMqSubscriber<T> : IRabbitMqSubscriber<T>, IDisposable
    {
        private readonly ILog _log;
        
        private RabbitMqBroker.Subscriber.RabbitMqSubscriber<T> _subscriber;
        
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly bool _isDurable;

        public RabbitMqSubscriber(ILog log, string connectionString, string exchangeName, string queueName, bool isDurable)
        {
            _log = log;
            _connectionString = connectionString;
            _exchangeName = exchangeName;
            _queueName = queueName;
            _isDurable = isDurable;
        }

        public void Subscribe(Func<T, Task> handleMessage)
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _connectionString,
                ExchangeName = _exchangeName,
                QueueName = _queueName,
                IsDurable = _isDurable,
            };

            _subscriber = new RabbitMqBroker.Subscriber.RabbitMqSubscriber<T>(settings,
                    new ResilientErrorHandlingStrategy(_log, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<T>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(handleMessage)
                .CreateDefaultBinding()
                .SetLogger(_log)
                .Start();
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }
    }
}

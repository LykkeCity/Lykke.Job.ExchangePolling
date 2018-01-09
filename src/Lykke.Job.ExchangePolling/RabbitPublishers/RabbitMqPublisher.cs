using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.ExchangePolling.RabbitPublishers
{
    internal class RabbitMqPublisher<T> : IRabbitMqPublisher<T>, IDisposable
    {
        private readonly bool _enabled;
        private readonly RabbitMqBroker.Publisher.RabbitMqPublisher<T> _rabbitPublisher;
        private readonly object _sync = new object();

        public RabbitMqPublisher(string connectionString, string exchangeName, bool enabled, ILog log, bool durable = true)
        {
            _enabled = enabled;
            if (!enabled)
            {
                log.WriteInfoAsync($"{GetType()}", "Constructor", $"A rabbit mq handler for {typeof(T)} is disabled");
                return;
            }
            var publisherSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = connectionString,
                ExchangeName = exchangeName,
                IsDurable = durable
            };

            _rabbitPublisher = new RabbitMqBroker.Publisher.RabbitMqPublisher<T>(publisherSettings)
                .DisableInMemoryQueuePersistence()
                .SetSerializer(new JsonMessageSerializer<T>())
                .SetLogger(log)
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(publisherSettings))
                .SetConsole(new LogToConsole())
                .PublishSynchronously()
                .Start();
        }

        public Task Publish(T message)
        {
            if (!_enabled)
            {
                return Task.CompletedTask;
            }
            lock (_sync)
            {
                return _rabbitPublisher.ProduceAsync(message);
            }
        }

        public void Dispose()
        {
            _rabbitPublisher?.Stop();
            _rabbitPublisher?.Dispose();
        }
    }
}

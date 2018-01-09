using System;
using System.Threading.Tasks;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    public interface IRabbitMqSubscriber<T>
    {
        void Subscribe(Func<T, Task> handleMessage);
    }
}

using System;
using System.Threading.Tasks;

namespace Lykke.Job.LykkeJob.Core.Services
{
    public interface IRabbitMqSubscriber<T>
    {
        void Subscribe(Func<T, Task> handleMessage);
    }
}

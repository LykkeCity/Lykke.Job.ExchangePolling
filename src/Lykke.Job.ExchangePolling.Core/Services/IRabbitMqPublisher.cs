using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Job.LykkeJob.Contract;

namespace Lykke.Job.LykkeJob.Core.Services
{
    public interface IRabbitMqPublisher<in T>
    {
        Task Publish(T message);
    }
}

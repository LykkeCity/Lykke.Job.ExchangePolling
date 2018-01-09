using System.Threading.Tasks;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    public interface IRabbitMqPublisher<in T>
    {
        Task Publish(T message);
    }
}

using System.Threading.Tasks;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}

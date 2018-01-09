using System.Threading.Tasks;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}

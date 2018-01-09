using System;
using System.Threading.Tasks;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    public interface IExchangePollingService
    {
        Task Poll(string exchangeName, TimeSpan timeout);
    }
}

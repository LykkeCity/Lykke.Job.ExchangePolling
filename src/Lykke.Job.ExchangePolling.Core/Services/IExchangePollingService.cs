using System;
using System.Threading.Tasks;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    /// <summary>
    /// Service contains polling logic
    /// </summary>
    public interface IExchangePollingService
    {
        /// <summary>
        /// Polling entry point
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task Poll(string exchangeName, TimeSpan timeout);
    }
}

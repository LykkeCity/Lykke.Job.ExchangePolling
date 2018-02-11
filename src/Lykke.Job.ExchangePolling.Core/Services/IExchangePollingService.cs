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
        /// Polling entry point for non-realtime exchanges.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task NonStreamingExchangePoll(string exchangeName, TimeSpan timeout);

        /// <summary>
        /// Polling entry point for position control long cycle.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task PositionControlPoll(string exchangeName, TimeSpan timeout);

        /// <summary>
        /// Position control starts this repeating poll to make sure that position divergence has taken place.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task PositionControlRepeatPoll(string exchangeName, TimeSpan timeout);
    }
}

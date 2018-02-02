using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Service.ExchangeConnector.Client;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public sealed class PositionControlPollingHandler : ExchangePollingHandler
    {
        public PositionControlPollingHandler(
            IExchangePollingService exchangePollingService,
            IExchangeConnectorService exchangeConnectorService,
            ILog log,
            int pollingPeriodMilliseconds)
            : base(nameof(ExchangePollingHandler), exchangePollingService.PositionControlPoll, pollingPeriodMilliseconds,
                exchangeConnectorService, log)
        {
        }

        /// <summary>
        /// Grab the list of non-streaming exchanges and start the Timer
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAndStart()
        {
            ExchangeNames = await ExchangeConnectorService.GetSupportedExchangesAsync();
            
            this.Start();
        }
    }
}

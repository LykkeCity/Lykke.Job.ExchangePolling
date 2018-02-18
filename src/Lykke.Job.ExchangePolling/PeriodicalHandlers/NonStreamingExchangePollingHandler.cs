using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Service.ExchangeConnector.Client;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public sealed class NonStreamingExchangePollingHandler : ExchangePollingHandler
    {
        public NonStreamingExchangePollingHandler(
            IExchangePollingService exchangePollingService,
            IExchangeConnectorService exchangeConnectorService,
            ILog log,
            int pollingPeriodMilliseconds)
            : base(nameof(NonStreamingExchangePollingHandler), 
                exchangePollingService.NonStreamingExchangePoll, 
                pollingPeriodMilliseconds,
                exchangeConnectorService, log)
        {
        }

        /// <summary>
        /// Grab the list of order non-streaming exchanges and start the Timer
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAndStart()
        {
            var extendedExchangeData = await this.RetrieveExchangeConfig();
            ExchangeNames = extendedExchangeData.Where(e => !(e.StreamingSupport.Orders ?? true)).Select(e => e.Name);

            await Log.WriteInfoAsync(nameof(NonStreamingExchangePollingHandler), nameof(InitializeAndStart),
                $"Initialized with non-streaming exchanges: {string.Join(", ", ExchangeNames)}", DateTime.UtcNow);
            
            this.Start();
        }
    }
}

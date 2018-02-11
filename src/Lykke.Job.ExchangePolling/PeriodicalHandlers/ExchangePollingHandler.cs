using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Service.ExchangeConnector.Client;
using Lykke.Service.ExchangeConnector.Client.Models;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public abstract class ExchangePollingHandler : TimerPeriod
    {
        protected IEnumerable<string> ExchangeNames { get; set; }

        private readonly Func<string, TimeSpan, Task> _pollingHandler;

        protected readonly TimeSpan PollingPeriod;
        
        protected readonly IExchangeConnectorService ExchangeConnectorService;
        
        private static IEnumerable<ExchangeInformationModel> _exchangeConfig;

        protected ExchangePollingHandler(
            string contextName,
            Func<string, TimeSpan, Task> pollingHandler,
            int pollingPeriodMilliseconds,
            IExchangeConnectorService exchangeConnectorService,
            ILog log)
            : base(contextName, pollingPeriodMilliseconds, log)
        {
            _pollingHandler = pollingHandler;
            
            PollingPeriod = TimeSpan.FromMilliseconds(pollingPeriodMilliseconds);

            ExchangeConnectorService = exchangeConnectorService;
        }

        protected async Task<IEnumerable<ExchangeInformationModel>> RetrieveExchangeConfig()
        {
            if (_exchangeConfig != null)
                return _exchangeConfig;
            
            var allExchanges = await ExchangeConnectorService.GetSupportedExchangesAsync();
            _exchangeConfig = await Task.WhenAll(allExchanges.Select(e =>
                ExchangeConnectorService.GetExchangeInfoAsync(e, new CancellationTokenSource(PollingPeriod).Token)));
            
            return _exchangeConfig;
        }

        public override async Task Execute()
        {
            await Task.WhenAll(ExchangeNames.Select(exchangeName => _pollingHandler(exchangeName, PollingPeriod)));
        }
    }
}

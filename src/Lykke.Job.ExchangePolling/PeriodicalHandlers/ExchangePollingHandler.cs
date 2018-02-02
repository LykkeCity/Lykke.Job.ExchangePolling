using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Service.ExchangeConnector.Client;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public abstract class ExchangePollingHandler : TimerPeriod
    {
        protected IEnumerable<string> ExchangeNames { get; set; }
        
        protected readonly IExchangeConnectorService ExchangeConnectorService;

        protected readonly TimeSpan PollingPeriod;

        private readonly Func<string, TimeSpan, Task> _pollingHandler;

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

        public override async Task Execute()
        {
            await Task.WhenAll(ExchangeNames.Select(exchangeName => _pollingHandler(exchangeName, PollingPeriod)));
        }
    }
}

using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public abstract class ExchangePollingHandler : TimerPeriod
    {
        private string ExchangeName { get; }

        private readonly IExchangePollingService _exchangePollingService;

        private readonly int _pollingPeriodMilliseconds;

        protected ExchangePollingHandler(
            IExchangePollingService exchangePollingService,
            ILog log,
            string exchangeName,
            int pollingPeriodMilliseconds)
            : base(nameof(ExchangePollingHandler), pollingPeriodMilliseconds, log)
        {
            _exchangePollingService = exchangePollingService;

            ExchangeName = exchangeName;

            _pollingPeriodMilliseconds = pollingPeriodMilliseconds;
        }

        public override async Task Execute()
        {
            await _exchangePollingService.Poll(ExchangeName, TimeSpan.FromMilliseconds(_pollingPeriodMilliseconds));
        }
    }
}

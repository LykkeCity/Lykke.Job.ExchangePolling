using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public class JfdPollingHandler : ExchangePollingHandler
    {
        public JfdPollingHandler(
            IExchangePollingService exchangePollingService,
            ILog log,
            int pollingPeriodMilliseconds)
            : base(exchangePollingService, log, "jfd", pollingPeriodMilliseconds)
        {

        }
    }
}

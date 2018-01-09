using Common.Log;
using Lykke.Job.LykkeJob.Core.Services;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public class IcmPollingHandler : ExchangePollingHandler
    {
        public IcmPollingHandler(
            IExchangePollingService exchangePollingService,
            ILog log,
            int pollingPeriodMilliseconds)
            : base(exchangePollingService, log, "icm", pollingPeriodMilliseconds)
        {

        }
    }
}

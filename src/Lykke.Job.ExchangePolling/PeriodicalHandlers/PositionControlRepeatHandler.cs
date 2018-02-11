using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Core;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Job.ExchangePolling.Core.Settings.JobSettings;
using Lykke.Service.ExchangeConnector.Client;
using Lykke.SettingsReader;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public class PositionControlRepeatHandler : ExchangePollingHandler, IPositionControlRepeatHandler
    {
        public string ExchangeName { get; private set; }
        public Dictionary<string, decimal> Divergence { get; private set; }

        private int _counter = 0;
        
        public PositionControlRepeatHandler(
            IExchangePollingService exchangePollingService,
            IExchangeConnectorService exchangeConnectorService,
            ILog log,
            IReloadingManager<ExchangePollingJobSettings> settings,
            string exchangeName,
            Dictionary<string, decimal> divergence)
            : base(nameof(PositionControlRepeatHandler), 
                exchangePollingService.PositionControlRepeatPoll, 
                Math.Min(settings.CurrentValue.PositionControlPollingPeriodMilliseconds, 
                    settings.CurrentValue.DivergenceRecheckTimeoutMilliseconds),
                exchangeConnectorService, log)
        {
            ExchangeName = exchangeName;
            Divergence = divergence;
        }

        bool IPositionControlRepeatHandler.Working => base.Working;

        void IPositionControlRepeatHandler.Start()
        {
            base.Start();
        }
        
        void IPositionControlRepeatHandler.Stop()
        {
            base.Stop();
        }

        public override async Task Execute()
        {
            if (_counter++ == 0)
                return;
            
            try
            {
                await base.Execute();
            }
            finally
            {
                this.Stop();
            }
        }
    }
}

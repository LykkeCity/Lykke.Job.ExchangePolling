﻿using System;
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
            : base(nameof(PositionControlPollingHandler), 
                exchangePollingService.PositionControlPoll, 
                pollingPeriodMilliseconds,
                exchangeConnectorService, log)
        {
        }

        /// <summary>
        /// Grab the list of order streaming exchanges and start the Timer
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAndStart()
        {
            var extendedExchangeData = await this.RetrieveExchangeConfig();
            ExchangeNames = extendedExchangeData.Where(e => (e.StreamingSupport.Orders ?? false)).Select(e => e.Name);

            await Log.WriteInfoAsync(nameof(PositionControlPollingHandler), nameof(InitializeAndStart),
                $"Initialized with streaming exchanges: {string.Join(", ", ExchangeNames)}", DateTime.UtcNow);

            this.Start();
        }
    }
}

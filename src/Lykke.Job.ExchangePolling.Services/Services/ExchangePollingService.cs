using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Lykke.Job.ExchangePolling.Core;
using Lykke.Job.ExchangePolling.Core.Domain;
using Lykke.Job.ExchangePolling.Services.Caches;
using Lykke.Job.LykkeJob.Contract;
using Lykke.Job.LykkeJob.Core.Services;
using Lykke.Service.ExchangeConnector.Client;

namespace Lykke.Job.LykkeJob.Services
{
    public class ExchangePollingService : IExchangePollingService
    {
        private readonly ExchangeCache _exchangeCache;

        private readonly IExchangeConnectorService _exchangeConnectorService;

        private readonly IRabbitMqPublisher<ExecutionReport> _executionReportPublisher;
        
        public ExchangePollingService(
            ExchangeCache exchangeCache,
            
            IExchangeConnectorService exchangeConnectorService,
            
            IRabbitMqPublisher<ExecutionReport> executionReportPublisher)
        {
            _exchangeCache = exchangeCache;

            _exchangeConnectorService = exchangeConnectorService;

            _executionReportPublisher = executionReportPublisher;
        }
        
        public async Task Poll(string exchangeName, TimeSpan timeout)
        {
            var tokenSource = new CancellationTokenSource(timeout);

            var positions = ((IEnumerable<Lykke.Service.ExchangeConnector.Client.Models.PositionModel>)
                    await _exchangeConnectorService.GetOpenedPositionAsync(exchangeName, tokenSource.Token))
                .Select(Position.Create).ToList();
            
            
        }

        /// <summary>
        /// Compare cached and new positions state, generate diff orders in case of divergence
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        public IEnumerable<ExecutedTrade> CheckPositions(string exchangeName, IEnumerable<Position> positions)
        {
            var exchange = _exchangeCache.Get(exchangeName);
            var allInstruments = (positions ?? new List<Position>())
                .Select(x => x.Symbol).Concat(exchange.Positions.Select(x => x.Symbol));
            var checkResults = allInstruments.Select(instrument =>
            {
                var delta = CheckSinglePosition(
                    exchange.Positions.FirstOrDefault(p => p.Symbol == instrument),
                    positions?.FirstOrDefault(p => p.Symbol == instrument));
                return (instrument, delta);
            }).Where(x => x.Item2 != 0);

            return checkResults.Select(x => CreateExecutedTrade(exchangeName, x.Item1, x.Item2));
        }

        private ExecutedTrade CreateExecutedTrade(string exchangeName, string instrument, decimal diff)
        {
            return new ExecutedTrade(new Instrument(exchangeName, instrument),
                DateTime.UtcNow,
                0, //where to get the price?
                diff,
                diff > 0 ? TradeType.Buy : TradeType.Sell,
                Constants.DiffOrderPrefix + Guid.NewGuid(),
                ExecutionStatus.Fill);
        }

        /// <summary>
        /// Compare new and old position, and return the difference
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="oldPosition"></param>
        /// <param name="newPosition"></param>
        /// <returns></returns>
        private decimal CheckSinglePosition(Position oldPosition, Position newPosition)
        {
            return (newPosition?.PositionVolume ?? 0) - (oldPosition?.PositionVolume ?? 0);
            /*
             *var delta = position.PositionVolume -
                                            (currentPosition?.PositionVolume ?? default(double));
                                var comparanceVolume = position.PositionVolume == default(double)
                                    ? currentPosition?.PositionVolume ?? default(double)
                                    : position.PositionVolume;
                                if (comparanceVolume == default(double)
                                    || Math.Abs(delta / comparanceVolume) < Constants.PositionChangedThreshold)
                                    continue;
             * 
             */
        }
    }
}

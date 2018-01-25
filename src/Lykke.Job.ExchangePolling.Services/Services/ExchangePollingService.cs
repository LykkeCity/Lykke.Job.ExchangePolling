using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Job.ExchangePolling.Core.Settings.JobSettings;
using Lykke.Service.ExchangeConnector.Client;
using Lykke.SettingsReader;
using Nito.AsyncEx;

namespace Lykke.Job.ExchangePolling.Services.Services
{
    public class ExchangePollingService : IExchangePollingService
    {
        private readonly IExchangeCache _exchangeCache;

        private readonly IQuoteService _quoteService;

        private readonly IExchangeConnectorService _exchangeConnectorService;

        private readonly IRabbitMqPublisher<ExecutionReport> _executionReportPublisher;
        
        private readonly IReloadingManager<ExchangePollingJobSettings> _settings;

        private readonly ILog _log;

        protected readonly AsyncLock _mutex = new AsyncLock();
        
        public ExchangePollingService(
            IExchangeCache exchangeCache,
            
            IQuoteService quoteService,
            
            IExchangeConnectorService exchangeConnectorService,
            
            IRabbitMqPublisher<ExecutionReport> executionReportPublisher,
            
            IReloadingManager<ExchangePollingJobSettings> settings,
            
            ILog log)
        {
            _exchangeCache = exchangeCache;

            _quoteService = quoteService;

            _exchangeConnectorService = exchangeConnectorService;

            _executionReportPublisher = executionReportPublisher;

            _settings = settings;

            _log = log;
        }

        public async Task Poll(string exchangeName, TimeSpan timeout)
        {
            var tokenSource = new CancellationTokenSource(timeout);
            
            using (await _mutex.LockAsync())//tokenSource.Token))
            {
                await PollEntryPoint(exchangeName, tokenSource.Token);
            }
        }
        
        private async Task PollEntryPoint(string exchangeName, CancellationToken cancellationToken)
        {
            List<Position> positions = null;
            
            //retrieve positions
            try
            {
                positions = (await _exchangeConnectorService.GetOpenedPositionAsync(exchangeName, cancellationToken))
                    .Select(Position.Create).ToList();
            }
            catch (Exception ex)
            {
                await _log.WriteWarningAsync(nameof(ExchangePollingService), nameof(Poll), 
                    $"{exchangeName} exchange polling failed.", ex, DateTime.UtcNow);
                //TODO consider sending slack notification here
                return;
            }
            
            var exchange = _exchangeCache.GetOrCreate(exchangeName);
            
            //perform checking
            var checkResults = CheckPositions(exchange, positions);
            
            //create diff order for Risk System
            var executedTrades = checkResults
                .Select(x => CreateExecutionReport(exchangeName, x.Item1, x.Item2))
                //do nothing if there's no quotes on any instrument, and wait for actual quotes.
                .Where(x => x != null)
                .ToList();

            if (executedTrades.Count == 0)
                return;

            //publish trades for Risk System
            foreach (var executedTrade in executedTrades)
                await _executionReportPublisher.Publish(executedTrade);
            await _log.WriteInfoAsync(nameof(ExchangePollingService), nameof(Poll),
                    $"Execution report have been published for {exchangeName}: {string.Join(", ", executedTrades.Select(x => x.Instrument.Name))}",
                    DateTime.UtcNow);
            
            //update positions and push published changes to cache
            exchange.UpdatePositions(positions.Where(x =>
                executedTrades.Select(tr => tr.Instrument.Name).Any(instrument => instrument == x.Symbol)));
            _exchangeCache.Set(exchange);
        }

        /// <summary>
        /// Compare cached and new positions state, generate diff results in case of divergence
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        private IEnumerable<(string instrument, decimal delta)> CheckPositions(Exchange exchange, 
            IEnumerable<Position> positions)
        {
            var allInstruments = (positions ?? new List<Position>())
                .Select(x => x.Symbol)
                .Concat(exchange.Positions.Select(x => x.Symbol))
                .Distinct();
            
            return allInstruments.Select(instrument =>
            {
                var delta = CheckSinglePosition(
                    exchange.Positions.FirstOrDefault(p => p.Symbol == instrument),
                    positions?.FirstOrDefault(p => p.Symbol == instrument));
                return (instrument, delta);
            }).Where(x => x.Item2 != 0);
        }

        /// <summary>
        /// Creates execution report for Risk System. Return null if there's no quotes.
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="instrument"></param>
        /// <param name="diff"></param>
        /// <returns></returns>
        private ExecutionReport CreateExecutionReport(string exchangeName, string instrument, decimal diff)
        {
            var quote = _quoteService.Get(exchangeName, instrument);
            if (quote == null)
            {
                _log.WriteWarningAsync(nameof(ExchangePollingService), nameof(CreateExecutionReport), 
                    $"Failed to get quotes for {exchangeName}: {instrument}. Stopped until next iteration.");
                return null;
            }

            var timeSpan = DateTime.UtcNow.Subtract(quote.Timestamp).TotalMilliseconds -
                           _settings.CurrentValue.QuotesTtlMilliseconds;
            if (timeSpan > 0)
                _log.WriteWarningAsync(nameof(ExchangePollingService), nameof(CreateExecutionReport), 
                    $"Quotes are outdated for {exchangeName}: {instrument}. Last quote was {timeSpan} milliseconds ago.");

            return new ExecutionReport(new Contract.Instrument(exchangeName, instrument),
                DateTime.UtcNow,
                diff > 0 ? quote.Bid : quote.Ask,
                diff,
                diff > 0 ? Contract.Enums.TradeType.Buy : Contract.Enums.TradeType.Sell,
                Constants.DiffOrderPrefix + Guid.NewGuid(),
                Contract.Enums.OrderExecutionStatus.Fill);
        }

        /// <summary>
        /// Compare new and old position, and return the difference
        /// </summary>
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

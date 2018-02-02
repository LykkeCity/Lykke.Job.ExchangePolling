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

        private readonly IPositionControlReportPublisher _positionControlReportPublisher;
        private readonly INonStreamingReportPublisher _nonStreamingReportPublisher;
        
        private readonly IReloadingManager<ExchangePollingJobSettings> _settings;

        private readonly ILog _log;

        private readonly AsyncLock _mutex = new AsyncLock();
        
        public ExchangePollingService(
            IExchangeCache exchangeCache,
            
            IQuoteService quoteService,
            
            IExchangeConnectorService exchangeConnectorService,
            
            IPositionControlReportPublisher positionControlReportPublisher,
            INonStreamingReportPublisher nonStreamingReportPublisher,
            
            IReloadingManager<ExchangePollingJobSettings> settings,
            
            ILog log)
        {
            _exchangeCache = exchangeCache;

            _quoteService = quoteService;

            _exchangeConnectorService = exchangeConnectorService;

            _positionControlReportPublisher = positionControlReportPublisher;
            _nonStreamingReportPublisher = nonStreamingReportPublisher;

            _settings = settings;

            _log = log;
        }

        public async Task PositionControlPoll(string exchangeName, TimeSpan timeout)
        {
            await PollAndHandleChanges(nameof(PositionControlPoll), _positionControlReportPublisher.Publish,
                exchangeName, timeout);
        }
        
        public async Task PollNonStreamingExchange(string exchangeName, TimeSpan timeout)
        {
            await PollAndHandleChanges(nameof(PollNonStreamingExchange), _nonStreamingReportPublisher.Publish,
                exchangeName, timeout);
        }

        private async Task PollAndHandleChanges(string context, Func<ExecutionReport, Task> publishFunc, 
            string exchangeName, TimeSpan timeout)
        {
            var positions = await RetrievePosition(context, exchangeName, timeout);
            if (positions == null)
                return;
            
            using (await _mutex.LockAsync()) //tokenSource.Token))
            {
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
                    await publishFunc(executedTrade);
                await _log.WriteInfoAsync(nameof(ExchangePollingService), context,
                    $"Execution report was published for {exchangeName}: {string.Join(", ", executedTrades.Select(x => x.Instrument.Name))}",
                    DateTime.UtcNow);

                //update positions and push published changes to cache
                exchange.UpdatePositions(positions.Where(x =>
                    executedTrades.Select(tr => tr.Instrument.Name).Any(instrument => instrument == x.Symbol)));
                _exchangeCache.Set(exchange);
            }
        }

        private async Task<IReadOnlyList<Position>> RetrievePosition(string context, string exchangeName, TimeSpan timeout)
        {
            try
            {
                return (await _exchangeConnectorService.GetOpenedPositionAsync(exchangeName,
                    new CancellationTokenSource(timeout).Token)).Select(Position.Create).ToList();
            }
            catch (Exception ex)
            {
                await _log.WriteWarningAsync(nameof(ExchangePollingService), context,
                    $"{exchangeName} exchange polling failed.", ex, DateTime.UtcNow);
                //TODO consider sending slack notification here
                return null;
            }
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
        }
    }
}

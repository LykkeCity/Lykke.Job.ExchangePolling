using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Job.ExchangePolling.Core.Settings.JobSettings;
using Lykke.SettingsReader;

namespace Lykke.Job.ExchangePolling.Services.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly IQuoteCache _quoteCache;

        private readonly ILog _log;

        private readonly List<string> _requiredExchanges;

        public QuoteService(IQuoteCache quoteCache, 
            IReloadingManager<ExchangePollingJobSettings> settings,
            ILog log)
        {
            _quoteCache = quoteCache;
            _log = log;

            _requiredExchanges = typeof(ExchangePollingJobSettings).GetProperties()
                .Where(x => x.PropertyType == typeof(ExchangeSettings))
                .Select(x => ((ExchangeSettings)x.GetValue(settings.CurrentValue)).ExchangeName)
                .ToList();
        }

        public async Task HandleQuote(OrderBook orderBook)
        {
            if (_requiredExchanges.All(x => x != orderBook.Source))
                return;
            
            var bestPriceQuote = ConvertToBestPriceQuote(orderBook);

            if (string.IsNullOrEmpty(bestPriceQuote?.ExchangeName) || string.IsNullOrEmpty(bestPriceQuote.Instrument)
                                                                   || bestPriceQuote.Bid == 0 || bestPriceQuote.Ask == 0)
            {
                await _log.WriteWarningAsync("QuoteService", "HandleQuote",
                    "Incoming quote is incorrect: " + (bestPriceQuote?.ToString() ?? "null"));
                return;
            }

            _quoteCache.Set(new ExchangeInstrumentQuote
            {
                ExchangeName = bestPriceQuote.ExchangeName,
                Instrument = bestPriceQuote.Instrument,
                Base = "",
                Quote = "",
                Bid = bestPriceQuote.Bid,
                Ask = bestPriceQuote.Ask
            });
        }

        public ExchangeInstrumentQuote Get(string exchangeName, string instrument)
        {
            return _quoteCache.Get(exchangeName, instrument);
        }

        private ExchangeBestPrice ConvertToBestPriceQuote(OrderBook orderBook)
        {
            var ask = GetBestPrice(true, orderBook.Asks);
            var bid = GetBestPrice(false, orderBook.Bids);
            
            return ask == null || bid == null
                ? null
                : new ExchangeBestPrice
            {
                ExchangeName = orderBook.Source,
                Instrument = orderBook.AssetPairId,
                Timestamp = orderBook.Timestamp,
                Ask = ask.Value,
                Bid = bid.Value
            };
        }

        private decimal? GetBestPrice(bool isBuy, IReadOnlyCollection<VolumePrice> prices)
        {
            if (!prices.Any())
                return null;
            return isBuy
                ? prices.Min(x => x.Price)
                : prices.Max(x => x.Price);
        }
    }
}

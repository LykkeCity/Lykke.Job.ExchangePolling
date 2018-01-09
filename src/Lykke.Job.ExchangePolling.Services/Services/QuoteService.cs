using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using Lykke.Job.LykkeJob.Core.Services;

namespace Lykke.Job.LykkeJob.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly IQuoteCache _quoteCache;

        private readonly ILog _log;

        public QuoteService(IQuoteCache quoteCache, ILog log)
        {
            _quoteCache = quoteCache;
            _log = log;
        }

        public async Task HandleQuote(ExchangeBestPrice quote)
        {
            if (string.IsNullOrEmpty(quote?.ExchangeName) || string.IsNullOrEmpty(quote.Instrument)
                                                          || quote.Bid == 0 || quote.Ask == 0)
            {
                await _log.WriteWarningAsync("QuoteService", "HandleQuote",
                    "Incoming quote is incorrect: " + (quote?.ToString() ?? "null"));
                return;
            }
            
            _quoteCache.Set(new ExchangeInstrumentQuote
            {
                ExchangeName = quote.ExchangeName,
                Instrument = quote.Instrument,
                Base = "",
                Quote = "",
                Bid = quote.Bid,
                Ask = quote.Ask
            });
        }

        public ExchangeInstrumentQuote Get(string exchangeName, string instrument)
        {
            return _quoteCache.Get(exchangeName, instrument);
        }
    }
}

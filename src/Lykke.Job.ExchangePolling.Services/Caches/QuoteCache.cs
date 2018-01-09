using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;

namespace Lykke.Job.ExchangePolling.Services.Caches
{
    public class QuoteCache : GenericDoubleDictionaryCache<ExchangeInstrumentQuote>, IQuoteCache
    {
        
    }
}

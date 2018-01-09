using Lykke.Job.ExchangePolling.Core.Domain;

namespace Lykke.Job.ExchangePolling.Core.Caches
{
    public interface IQuoteCache : IGenericDoubleDictionaryCache<ExchangeInstrumentQuote>
    {
        
    }
}

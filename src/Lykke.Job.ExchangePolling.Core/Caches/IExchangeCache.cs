using System.Collections.Generic;
using Lykke.Job.ExchangePolling.Core.Domain;

namespace Lykke.Job.ExchangePolling.Core.Caches
{
    public interface IExchangeCache : IGenericDictionaryCache<Exchange>
    {
        Exchange GetOrCreate(string exchangeName);
        
        /// <summary>
        /// Initializes cache from saved state, substituting position with ones from Hedging Service.
        /// Returns new state of cache to be saved to blob.
        /// </summary>
        /// <param name="savedCache"></param>
        /// <param name="positionsFromHedging"></param>
        IReadOnlyList<Exchange> Initialize(IReadOnlyCollection<Exchange> savedCache,
            Dictionary<string, List<Position>> positionsFromHedging);
    }
}

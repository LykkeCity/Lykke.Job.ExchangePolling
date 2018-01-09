using System.Collections.Generic;
using System.Linq;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using MoreLinq;

namespace Lykke.Job.ExchangePolling.Services.Caches
{
    public class ExchangeCache : GenericDictionaryCache<Exchange>, IExchangeCache
    {

        public IReadOnlyList<Exchange> Initialize(IEnumerable<Exchange> savedCache,
            Dictionary<string, List<Position>> allPositionsFromHedging)
        {
            //substitute exchange positions with ones from Hedging System API
            var preparedCache = savedCache.Select(exchange =>
            {
                allPositionsFromHedging.TryGetValue(exchange.Name, out var positionsFromHedging);
                var mergedPositions = exchange.Positions.Select(pos =>
                    pos.Merge(positionsFromHedging?.FirstOrDefault(y => y.Symbol == pos.Symbol))
                    ).ToList();
                
                exchange.Positions = mergedPositions;
                
                return exchange;
            }).ToList();
            
            base.Initialize(preparedCache);
            
            return preparedCache;
        }
    }
}

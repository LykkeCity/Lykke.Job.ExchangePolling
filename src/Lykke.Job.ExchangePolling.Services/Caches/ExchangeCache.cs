using System.Collections.Generic;
using System.Linq;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using MoreLinq;

namespace Lykke.Job.ExchangePolling.Services.Caches
{
    public class ExchangeCache : GenericDictionaryCache<Exchange>, IExchangeCache
    {
        public Exchange GetOrCreate(string exchangeName)
        {
            var exchange = this.Get(exchangeName);
            if (exchange == null)
            {
                exchange = new Exchange(exchangeName);
                this.Set(exchange);
            }
            return exchange;
        }
        
        public IReadOnlyList<Exchange> Initialize(IReadOnlyCollection<Exchange> savedCache,
            Dictionary<string, List<Position>> allPositionsFromHedging)
        {
            var preparedCache = new List<Exchange>();
            //loop over exchange names
            savedCache.Select(x => x.Name).Concat(allPositionsFromHedging.Keys).Distinct()
                .ForEach(exchange =>
                {
                    allPositionsFromHedging.TryGetValue(exchange, out var positionsFromHedging);

                    var savedExchange = savedCache.FirstOrDefault(x => x.Name == exchange);
                    var instruments = (savedExchange?.Positions?.Select(x => x.Symbol).ToList() ?? new List<string>())
                        .Concat(positionsFromHedging?.Select(x => x.Symbol) ?? new string[0])
                        .Distinct();

                    //merge positions in each instrument
                    var preparedPositions = instruments.Select(symbol => Position.Merge(
                        savedExchange?.Positions?.FirstOrDefault(x => x.Symbol == symbol),
                        positionsFromHedging?.FirstOrDefault(y => y.Symbol == symbol))).ToList();

                    var preparedExchange = savedExchange ?? new Exchange(exchange);
                    preparedExchange.Positions = preparedPositions;
                    preparedCache.Add(preparedExchange);
                });

            base.Initialize(preparedCache);

            return preparedCache;
        }
    }
}

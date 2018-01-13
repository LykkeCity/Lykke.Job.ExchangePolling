using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lykke.Job.ExchangePolling.Core.Caches;
using Newtonsoft.Json;

namespace Lykke.Job.ExchangePolling.Core.Domain
{
    public class Exchange : IKeyedObject, ICloneable
    {
        public string Name { get; private set; }
        
        public IReadOnlyList<AccountBalance> Accounts { get; set; }
        
        public IReadOnlyList<Position> Positions { get; set; }
        
        [JsonIgnore]
        public string GetKey => Name;

        public Exchange(string name)
        {
            Name = name;
            Accounts = new List<AccountBalance>();
            Positions = new List<Position>();
        }
        
        public object Clone()
        {
            return new Exchange(this.Name)
            {
                Accounts = this.Accounts?.Select(x => x.Clone()).ToList(),
                Positions = this.Positions?.Select(x => x.Clone()).ToList()
            };
        }

        public void UpdatePositions(IEnumerable<Position> updatedPositions)
        {
            this.Positions = this.Positions.ToList()
                .Where(x => updatedPositions.All(pos => pos.Symbol != x.Symbol))
                .Concat(updatedPositions)
                .ToList();
        }

        public override string ToString()
        {
            return $"{Name}: {string.Join(", ", Positions.Select(x => x.Symbol))}";
        }
    }
}

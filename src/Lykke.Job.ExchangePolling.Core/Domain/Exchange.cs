using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lykke.Job.ExchangePolling.Core.Caches;

namespace Lykke.Job.ExchangePolling.Core.Domain
{
    public class Exchange : IKeyedObject, ICloneable
    {
        public string Name { get; private set; }
        
        public IReadOnlyList<AccountBalance> Accounts { get; set; }
        
        public IReadOnlyList<Position> Positions { get; set; }
        
        public string GetKey => Name;
        
        public object Clone()
        {
            return new Exchange
            {
                Name = this.Name,
                Accounts = this.Accounts.Select(x => x.Clone()).ToList(),
                Positions = this.Positions.Select(x => x.Clone()).ToList()
            };
        }
    }
}

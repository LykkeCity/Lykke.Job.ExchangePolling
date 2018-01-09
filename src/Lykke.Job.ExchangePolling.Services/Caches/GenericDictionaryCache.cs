using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using MoreLinq;

namespace Lykke.Job.ExchangePolling.Services.Caches
{
    /// <summary>
    /// Immutable thread-safe generic cache based on dictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericDictionaryCache<T> : IGenericDictionaryCache<T>
        where T: class, IKeyedObject, ICloneable
    {
        private Dictionary<string, T> _cache;

        private static readonly object LockObj = new object();

        protected GenericDictionaryCache()
        {
            ClearAll();
        }
        
        public T Get(string key)
        {
            lock (LockObj)
            {
                return _cache.ContainsKey(key) ? (T)_cache[key].Clone() : null;
            }
        }

        public IReadOnlyList<T> GetAll()
        {
           lock(LockObj)
           {
               return _cache.Values.Select(x => (T)x.Clone()).ToList();
           }
        }

        public void Set(T item)
        {
            if (item == null)
                return;
            
            lock (LockObj)
            {
                _cache[item.GetKey] = (T)item.Clone();
            }
        }

        public void SetAll(IEnumerable<T> items)
        {
            if (items == null)
                return;
            
            lock (LockObj)
            {
                items.Where(x => x != null).ForEach(x => _cache[x.GetKey] = (T)x.Clone());
            }
        }

        public void Clear(string key)
        {
            lock (LockObj)
            {
                _cache.Remove(key);
            }
        }

        public void ClearAll()
        {
            lock (LockObj)
            {
                _cache = new Dictionary<string, T>();
            }
        }

        public void Initialize(IEnumerable<T> items)
        {
            ClearAll();
            SetAll(items);
        }
    }
}

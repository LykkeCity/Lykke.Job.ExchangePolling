using System.Collections.Generic;

namespace Lykke.Job.ExchangePolling.Core.Caches
{
    public interface IGenericDoubleDictionaryCache<T>
    {
        T Get(string partitionKey, string rowKey);
        
        IReadOnlyList<T> Get(string partitionKey);

        IReadOnlyList<T> GetAll();

        void Set(T item);

        void SetAll(IEnumerable<T> items);

        void Clear(string partitionKey, string rowKey);

        void ClearAll();

        void Initialize(IEnumerable<T> items);
    }
}

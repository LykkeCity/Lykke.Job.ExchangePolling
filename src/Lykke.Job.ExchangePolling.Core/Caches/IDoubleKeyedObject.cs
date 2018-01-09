namespace Lykke.Job.ExchangePolling.Core.Caches
{
    public interface IDoubleKeyedObject
    {
        string GetPartitionKey { get; }
        string GetRowKey { get; }
    }
}

namespace Lykke.Job.ExchangePolling.Core.Domain
{
    public interface IDoubleKeyedObject
    {
        string GetPartitionKey { get; }
        string GetRowKey { get; }
    }
}

namespace Lykke.Job.ExchangePolling.Core.Caches
{
    public interface IKeyedObject
    {
        string GetKey { get; }
    }
}

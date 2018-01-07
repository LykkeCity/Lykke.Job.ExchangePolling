namespace Lykke.Job.ExchangePolling.Core.Domain
{
    public interface IKeyedObject
    {
        string GetKey { get; }
    }
}

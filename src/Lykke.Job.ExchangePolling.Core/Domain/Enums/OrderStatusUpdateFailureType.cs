namespace Lykke.Job.ExchangePolling.Core.Domain.Enums
{
    public enum OrderStatusUpdateFailureType
    {
        None,
        Unknown,
        ExchangeError,
        ConnectorError,
        InsufficientFunds
    }
}

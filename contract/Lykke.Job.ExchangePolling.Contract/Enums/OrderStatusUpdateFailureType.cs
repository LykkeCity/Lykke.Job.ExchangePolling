namespace Lykke.Job.ExchangePolling.Contract.Enums
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

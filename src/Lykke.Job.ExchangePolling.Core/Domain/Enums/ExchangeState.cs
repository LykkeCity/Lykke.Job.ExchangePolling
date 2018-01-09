namespace Lykke.Job.ExchangePolling.Core.Domain.Enums
{
    public enum ExchangeState
    {
        Initializing,
        Connecting,
        ReconnectingAfterError,
        Connected,
        ReceivingPrices,
        ExecuteOrders,
        ErrorState,
        Stopped,
        Stopping
    }
}

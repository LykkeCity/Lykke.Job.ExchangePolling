namespace Lykke.Job.ExchangePolling.Contract.Enums
{
    public enum OrderExecutionStatus
    {
        Unknown,
        Fill,
        PartialFill,
        Cancelled,
        Rejected,
        New,
        Pending
    }
}

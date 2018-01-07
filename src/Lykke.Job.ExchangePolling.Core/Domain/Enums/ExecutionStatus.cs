namespace Lykke.Job.ExchangePolling.Core.Domain
{
    public enum ExecutionStatus
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

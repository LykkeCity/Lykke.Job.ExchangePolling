namespace Lykke.Job.ExchangePolling.Core.Domain.Enums
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

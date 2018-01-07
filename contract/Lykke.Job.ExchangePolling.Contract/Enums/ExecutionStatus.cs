namespace Lykke.Job.ExchangePolling.Contract.Enums
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

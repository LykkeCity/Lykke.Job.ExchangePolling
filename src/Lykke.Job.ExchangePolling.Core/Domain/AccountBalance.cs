namespace Lykke.Job.ExchangePolling.Core.Domain
{
    public class AccountBalance
    {
        public string AccountCurrency { get; set; }

        public double Totalbalance { get; set; }

        public double UnrealisedPnL { get; set; }

        public double MarginAvailable { get; set; }

        public double MarginUsed { get; set; }

        public AccountBalance Clone()
        {
            return new AccountBalance
            {
                AccountCurrency = this.AccountCurrency,
                Totalbalance = this.Totalbalance,
                UnrealisedPnL = this.UnrealisedPnL,
                MarginAvailable = this.MarginAvailable,
                MarginUsed = this.MarginUsed
            };
        }
    }
}

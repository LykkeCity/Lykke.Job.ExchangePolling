namespace Lykke.Job.ExchangePolling.Core.Settings.JobSettings
{
    public class ExchangePollingJobSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings Rabbit { get; set; }
        
        public ServicesSettings Services { get; set; }
        
        public ExchangeSettings JfdSettings { get; set; }
        
        public ExchangeSettings IcmSettings { get; set; }
        
        public int ExchangePollingPeriodMilliseconds { get; set; }
        
        public int DataSavingPeriodMilliseconds { get; set; }
        
        public int QuotesTtlMilliseconds { get; set; }
    }
}

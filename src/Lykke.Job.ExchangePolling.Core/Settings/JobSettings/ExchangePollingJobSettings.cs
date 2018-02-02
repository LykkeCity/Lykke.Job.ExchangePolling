namespace Lykke.Job.ExchangePolling.Core.Settings.JobSettings
{
    public class ExchangePollingJobSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings Rabbit { get; set; }
        
        public ServicesSettings Services { get; set; }
        
        public int NonStreamingPollingPeriodMilliseconds { get; set; }
        
        public int PositionControlPollingPeriodMilliseconds { get; set; }
        
        public int DataSavingPeriodMilliseconds { get; set; }
        
        public int QuotesTtlMilliseconds { get; set; }
    }
}

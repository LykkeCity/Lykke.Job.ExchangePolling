﻿namespace Lykke.Job.LykkeJob.Core.Settings.JobSettings
{
    public class ExchangePollingJobSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings Rabbit { get; set; }
        
        public ServicesSettings Services { get; set; }
        
        public ExchangeSettings BitmexSettings { get; set; }
    }
}

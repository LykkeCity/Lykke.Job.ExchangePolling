using System;

namespace Lykke.Job.ExchangePolling.Contract
{
    public class ExchangeBestPrice
    {
        public string ExchangeName { get; set; }
        
        public string Instrument { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public decimal Bid { get; set; }
        
        public decimal Ask { get; set; }
    }
}

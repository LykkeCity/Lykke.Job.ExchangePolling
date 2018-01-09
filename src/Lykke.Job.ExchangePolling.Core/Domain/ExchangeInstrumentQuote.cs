using System;

namespace Lykke.Job.ExchangePolling.Core.Domain
{
    public class ExchangeInstrumentQuote : IDoubleKeyedObject, ICloneable
    {
        public string ExchangeName { get; set; }
        
        public string Instrument { get; set; }
        
        public string @Base { get; set; }
        
        public string Quote { get; set; }
        
        public decimal Bid { get; set; }
        
        public decimal Ask { get; set; }

        public string GetPartitionKey => ExchangeName;

        public string GetRowKey => Instrument;
        
        public object Clone()
        {
            return new ExchangeInstrumentQuote
            {
                ExchangeName = this.ExchangeName,
                Instrument = this.Instrument,
                Base = this.Base,
                Quote = this.Quote,
                Bid = this.Bid,
                Ask = this.Ask
            };
        }
    }
}

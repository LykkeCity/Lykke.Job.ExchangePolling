namespace Lykke.Job.ExchangePolling.Core.Settings.JobSettings
{
    public class ServicesSettings
    {
        public RestServiceSettings ExchangeConnectorService { get; set; }
        
        public RestServiceSettings AggregatedHedgingService { get; set; }
    }
}

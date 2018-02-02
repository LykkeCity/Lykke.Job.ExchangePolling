using Common.Log;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core.Services;

namespace Lykke.Job.ExchangePolling.RabbitPublishers
{
    public class PositionControlReportPublisher: RabbitMqPublisher<ExecutionReport>, IPositionControlReportPublisher
    {
        public PositionControlReportPublisher(string connectionString, 
            string exchangeName, 
            bool enabled, 
            ILog log,
            bool durable = true) 
            : base(connectionString, exchangeName, enabled, log, durable)
        {
            
        }
        
         
    }
}

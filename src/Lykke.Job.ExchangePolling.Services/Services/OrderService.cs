using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Service.ExchangeConnector.Client.Models;

namespace Lykke.Job.ExchangePolling.Services.Services
{
    public class OrderService : IOrderService
    {
        
        private readonly ILog _log;

        public OrderService(
            ILog log)
        {
            _log = log;
        }

        public async Task HandleHedgingTrade(ExecutionReport trade)
        {
            if (!trade.Success)
                return;
            
            //resolve intended exchange and just proceed with an unscheduled Poll
            
        }
    }
}

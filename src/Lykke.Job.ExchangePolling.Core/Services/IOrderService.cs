using System.Threading.Tasks;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// All executed orders must be taken into account.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Task HandleHedgingTrade(Service.ExchangeConnector.Client.Models.ExecutionReport arg);
    }
}

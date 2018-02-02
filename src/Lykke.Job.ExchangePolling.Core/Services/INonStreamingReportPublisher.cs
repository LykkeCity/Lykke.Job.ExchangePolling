using System.Threading.Tasks;
using Lykke.Job.ExchangePolling.Contract;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    public interface INonStreamingReportPublisher
    {
        Task Publish(ExecutionReport executionReport);
    }
}

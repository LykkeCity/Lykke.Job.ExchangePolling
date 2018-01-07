using System;
using System.Threading.Tasks;

namespace Lykke.Job.LykkeJob.Core.Services
{
    public interface IExchangePollingService
    {
        Task Poll(string exchangeName, TimeSpan timeout);
    }
}

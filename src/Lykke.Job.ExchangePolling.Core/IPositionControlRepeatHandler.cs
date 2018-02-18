using System.Collections.Generic;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Service.ExchangeConnector.Client;

namespace Lykke.Job.ExchangePolling.Core
{
    public interface IPositionControlRepeatHandler
    {
        bool Working { get; }
        void Stop();
        void Start();
        
        string ExchangeName { get; }
        Dictionary<string, decimal> Divergence { get; }
    }
}

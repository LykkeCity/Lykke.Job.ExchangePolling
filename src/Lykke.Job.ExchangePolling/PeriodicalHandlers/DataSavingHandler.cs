using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.ExchangePolling.Core;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Repositories;

namespace Lykke.Job.ExchangePolling.PeriodicalHandlers
{
    public class DataSavingHandler : TimerPeriod
    {
        private readonly IExchangeCache _exchangeCache;

        private readonly IGenericBlobRepository _genericBlobRepository;
        
        private readonly int _periodMilliseconds;

        private readonly ILog _log;

        public DataSavingHandler(
            IExchangeCache exchangeCache,
            IGenericBlobRepository genericBlobRepository,
            ILog log,
            int periodMilliseconds)
            : base(nameof(DataSavingHandler), periodMilliseconds, log)
        {
            _exchangeCache = exchangeCache;

            _genericBlobRepository = genericBlobRepository;
            
            _periodMilliseconds = periodMilliseconds;

            _log = log;
        }

        public override async Task Execute()
        {
            var data = _exchangeCache.GetAll();
            if(data == null || data.Count == 0)
                return;
            
            await _genericBlobRepository.Write(Constants.BlobContainerName, Constants.BlobExchangesCache, data);
            await _log.WriteInfoAsync(nameof(DataSavingHandler), nameof(Execute), 
                $"Exchange cache saved to blob: {string.Join("; ", data)}.", DateTime.UtcNow);
        }
    }
}

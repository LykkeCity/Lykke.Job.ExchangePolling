using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Core;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using Lykke.Job.LykkeJob.Core.Services;

namespace Lykke.Job.ExchangePolling.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private readonly IExchangeCache _exchangeCache;

        private readonly IGenericBlobRepository _genericBlobRepository;
        
        private readonly ILog _log;

        public StartupManager(
            IExchangeCache exchangeCache,
            
            IGenericBlobRepository genericBlobRepository,
            
            ILog log)
        {
            _exchangeCache = exchangeCache;

            _genericBlobRepository = genericBlobRepository;
            
            _log = log;
        }

        public async Task StartAsync()
        {
            // TODO: Implement your startup logic here. Good idea is to log every step
            
            //TODO initialize prices
            
            
            //initialize ExchangeCache
            var savedExchanges =
                await _genericBlobRepository.ReadAsync<List<Exchange>>(Constants.BlobContainerName,
                    Constants.BlobExchangesCache);
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), 
                $"ExchangeCache initialized with: {savedExchanges?.Count.ToString() ?? "null"} elements.", DateTime.UtcNow);
            _exchangeCache.Initialize(savedExchanges);
            
            await Task.CompletedTask;
        }
    }
}

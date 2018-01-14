using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Core;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using Lykke.Job.ExchangePolling.Core.Repositories;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Job.ExchangePolling.Core.Settings;
using Lykke.Job.ExchangePolling.Core.Settings.JobSettings;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Contracts;
using MarginTrading.RiskManagement.HedgingService.Contracts.Client;
using MarginTrading.RiskManagement.HedgingService.Contracts.Models;

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
        private readonly IQuoteCache _quoteCache;

        private readonly IGenericBlobRepository _genericBlobRepository;

        private readonly IHedgingServiceClient _hedgingServiceClient;
        
        private readonly ILog _log;

        private readonly List<string> _requiredExchanges;

        public StartupManager(
            IExchangeCache exchangeCache,
            IQuoteCache quoteCache,
            
            IGenericBlobRepository genericBlobRepository,
            
            IHedgingServiceClient hedgingServiceClient,
            
            IReloadingManager<ExchangePollingJobSettings> settings,
            
            ILog log)
        {
            _exchangeCache = exchangeCache;
            _quoteCache = quoteCache;
            
            _genericBlobRepository = genericBlobRepository;

            _hedgingServiceClient = hedgingServiceClient;
            
            _log = log;

            _requiredExchanges = settings.CurrentValue.GetHandledExchanges().ToList();
        }

        /// <summary>
        /// Startup logic implementation.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            //read last saved cache
            var savedExchanges = await _genericBlobRepository.ReadAsync<List<Exchange>>(Constants.BlobContainerName,
                Constants.BlobExchangesCache);
            
            //retrieve current hedging positions
            IReadOnlyList<ExternalPositionModel> currentHedgingPositions = null;
            try
            {
                currentHedgingPositions = await _hedgingServiceClient.ExternalPositions.List();
                if (currentHedgingPositions == null)
                    throw new Exception("Error retrieving data from Hedging System.");
            }
            catch (Exception ex)
            {
                await _log.WriteFatalErrorAsync(nameof(StartupManager), "Retrieving hedging positions", ex, DateTime.UtcNow);
                throw;
            }

            //initialize ExchangeCache
            var cachedData = _exchangeCache.Initialize(savedExchanges,
                currentHedgingPositions?
                    .Where(x => _requiredExchanges.Any(exch => exch == x.Exchange))
                    .GroupBy(x => x.Exchange)
                    .ToDictionary(x => x.Key, x => x.Select(Position.Create).ToList())
                ?? new Dictionary<string, List<Position>>());
            
            //save old blob data
            await _genericBlobRepository.Write(Constants.BlobContainerName, 
                $"{Constants.BlobExchangesCache}_{DateTime.UtcNow:s}", savedExchanges);
            //write new blob data
            await _genericBlobRepository.Write(Constants.BlobContainerName, Constants.BlobExchangesCache, cachedData);

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), 
                $"ExchangeCache initialized with data: {string.Join("; ",cachedData)}.", DateTime.UtcNow);
        }
    }
}

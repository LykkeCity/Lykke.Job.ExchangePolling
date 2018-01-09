using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.ExchangePolling.Core;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Domain;
using Lykke.Job.LykkeJob.Core.Services;
using MarginTrading.MarketMaker.Contracts;
using MarginTrading.RiskManagement.HedgingService.Contracts.Client;

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
        
        private readonly ILog _log;

        public StartupManager(
            IExchangeCache exchangeCache,
            IQuoteCache quoteCache,
            
            IGenericBlobRepository genericBlobRepository,
            
            ILog log)
        {
            _exchangeCache = exchangeCache;
            _quoteCache = quoteCache;
            
            _genericBlobRepository = genericBlobRepository;
            
            _log = log;
        }

        /// <summary>
        /// Startup logic implementation.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            //initialize prices from MM API
            //IMtMarketMakerClient a; a.ExtPriceStatus.List()
            
            var quotes = new List<ExchangeInstrumentQuote>();//TODO get quotes here
            _quoteCache.Initialize(quotes);
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync),
                $"QuotesCached initialized with quotes from: {string.Join(", ", quotes.Select(x => x.ExchangeName))}");
            
            //initialize ExchangeCache
            var savedExchanges =
                await _genericBlobRepository.ReadAsync<List<Exchange>>(Constants.BlobContainerName,
                    Constants.BlobExchangesCache);
            //TODO substitute exchange positions with ones from Hedging System API
            //IHedgingServiceClient a;a.HedgingPosition.
            
            _exchangeCache.Initialize(savedExchanges);
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), 
                $"ExchangeCache initialized with data of: {savedExchanges?.Count.ToString() ?? "null"} echanges.", DateTime.UtcNow);
            
            await Task.CompletedTask;
        }
    }
}

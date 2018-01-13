using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.ExchangePolling.AzureRepositories;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Core.Repositories;
using Lykke.Job.ExchangePolling.Core.Services;
using Lykke.Job.ExchangePolling.Core.Settings.JobSettings;
using Lykke.Job.ExchangePolling.PeriodicalHandlers;
using Lykke.Job.ExchangePolling.RabbitPublishers;
using Lykke.Job.ExchangePolling.RabbitSubscribers;
using Lykke.Job.ExchangePolling.Services;
using Lykke.Job.ExchangePolling.Services.Caches;
using Lykke.Job.ExchangePolling.Services.Services;
using Lykke.Service.ExchangeConnector.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;

namespace Lykke.Job.ExchangePolling.Modules
{
    public class JobModule : Module
    {
        private readonly IReloadingManager<ExchangePollingJobSettings> _settings;
        private readonly bool _isDevelopment;

        private readonly ILog _log;

        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public JobModule(IReloadingManager<ExchangePollingJobSettings> settings,
            bool isDevelopment, 
            ILog log)
        {
            _settings = settings;
            _isDevelopment = isDevelopment;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // NOTE: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            // builder.RegisterType<QuotesPublisher>()
            //  .As<IQuotesPublisher>()
            //  .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString))

            builder.RegisterInstance(_settings)
                .As<IReloadingManager<ExchangePollingJobSettings>>()
                .SingleInstance();
            
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();
            
            RegisterPeriodicalHandlers(builder);
            
            RegisterRabbitMqSubscribers(builder);

            RegisterRabbitMqPublishers(builder);

            builder.Register<IGenericBlobRepository>(ctx =>
                    new GenericBlobRepository(_settings.Nested(x => x.Db.BlobConnString)))
                .SingleInstance();

            builder.RegisterType<ExchangePollingService>()
                .As<IExchangePollingService>()
                .SingleInstance();

            builder.RegisterType<QuoteService>()
                .As<IQuoteService>()
                .SingleInstance();

            builder.RegisterType<ExchangeCache>()
                .As<IExchangeCache>()
                .SingleInstance();
            
            builder.RegisterType<QuoteCache>()
                .As<IQuoteCache>()
                .SingleInstance();
            
            builder.RegisterType<ExchangeConnectorService>()
                .As<IExchangeConnectorService>()
                .WithParameter("settings",new ExchangeConnectorServiceSettings
                {
                    ServiceUrl = _settings.CurrentValue.Services.ExchangeConnectorService.Url,
                    ApiKey = _settings.CurrentValue.Services.ExchangeConnectorService.ApiKey
                })
                .SingleInstance();
            
            builder.Populate(_services);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<JfdPollingHandler>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.JfdSettings.PollingPeriodMilliseconds))
                .SingleInstance();
            builder.RegisterType<IcmPollingHandler>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.IcmSettings.PollingPeriodMilliseconds))
                .SingleInstance();
            
            builder.RegisterType<DataSavingHandler>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.DataSavingPeriodMilliseconds))
                .SingleInstance();
        }
        
        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<OrderBookSubscriber>()
                .AsSelf()
                .SingleInstance()
                .WithParameters(new[]
                {
                    new NamedParameter("connectionString", _settings.CurrentValue.Rabbit.ExchangeConnectorQuotesBTCUSD.ConnectionString),
                    new NamedParameter("exchangeName", _settings.CurrentValue.Rabbit.ExchangeConnectorQuotesBTCUSD.ExchangeName),
                    new NamedParameter("queueName", _settings.CurrentValue.Rabbit.ExchangeConnectorQuotesBTCUSD.QueueName),
                    new NamedParameter("isDurable", false),//!_isDevelopment),
                    new NamedParameter("log", _log)
                });
        }

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            builder.RegisterType<RabbitMqPublisher<ExecutionReport>>()
                .As<IRabbitMqPublisher<ExecutionReport>>()
                .SingleInstance()
                .WithParameters(new[]
                {
                    new NamedParameter("connectionString", _settings.CurrentValue.Rabbit.ExchangeConnectorOrder.ConnectionString),
                    new NamedParameter("exchangeName", _settings.CurrentValue.Rabbit.ExchangeConnectorOrder.ExchangeName),
                    new NamedParameter("enabled", true),
                    new NamedParameter("log", _log)
                });
        }
    }
}

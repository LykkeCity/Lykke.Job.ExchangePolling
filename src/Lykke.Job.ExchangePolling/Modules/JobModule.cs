using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.ExchangePolling.AzureRepositories;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core;
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

            builder.RegisterType<OrderService>()
                .As<IOrderService>()
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
            builder.RegisterType<NonStreamingExchangePollingHandler>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.NonStreamingPollingPeriodMilliseconds))
                .SingleInstance();
            
            builder.RegisterType<PositionControlPollingHandler>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PositionControlPollingPeriodMilliseconds))
                .SingleInstance();

            builder.RegisterType<PositionControlRepeatHandler>()
                .WithParameter(TypedParameter.From(_settings))
                .As<IPositionControlRepeatHandler>()
                .InstancePerDependency();
            
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
                    new NamedParameter("connectionString", _settings.CurrentValue.Rabbit.ExchangeConnectorQuotes.ConnectionString),
                    new NamedParameter("exchangeName", _settings.CurrentValue.Rabbit.ExchangeConnectorQuotes.ExchangeName),
                    new NamedParameter("queueName", _settings.CurrentValue.Rabbit.ExchangeConnectorQuotes.QueueName),
                    new NamedParameter("isDurable", false),
                    new NamedParameter("log", _log)
                });
            
            builder.RegisterType<ExchangeConnectorOrderSubscriber>()
                .AsSelf()
                .SingleInstance()
                .WithParameters(new[]
                {
                    new NamedParameter("connectionString", _settings.CurrentValue.Rabbit.ExchangeConnectorOrder.ConnectionString),
                    new NamedParameter("exchangeName", _settings.CurrentValue.Rabbit.ExchangeConnectorOrder.ExchangeName),
                    new NamedParameter("queueName", _settings.CurrentValue.Rabbit.ExchangeConnectorOrder.QueueName),
                    new NamedParameter("isDurable", !_isDevelopment),
                    new NamedParameter("log", _log)
                });
        }

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            builder.RegisterType<PositionControlReportPublisher>()
                .As<IPositionControlReportPublisher>()
                .SingleInstance()
                .WithParameters(new[]
                {
                    new NamedParameter("connectionString", _settings.CurrentValue.Rabbit.PositionControlOrder.ConnectionString),
                    new NamedParameter("exchangeName", _settings.CurrentValue.Rabbit.PositionControlOrder.ExchangeName),
                    new NamedParameter("enabled", true),
                    new NamedParameter("log", _log)
                });
            
            builder.RegisterType<NonStreamingReportPublisher>()
                .As<INonStreamingReportPublisher>()
                .SingleInstance()
                .WithParameters(new[]
                {
                    new NamedParameter("connectionString", _settings.CurrentValue.Rabbit.NonStreamingOrder.ConnectionString),
                    new NamedParameter("exchangeName", _settings.CurrentValue.Rabbit.NonStreamingOrder.ExchangeName),
                    new NamedParameter("enabled", true),
                    new NamedParameter("log", _log)
                });
        }
    }
}

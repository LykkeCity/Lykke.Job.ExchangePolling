using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.ExchangePolling.Core.Caches;
using Lykke.Job.ExchangePolling.Services.Caches;
using Lykke.Job.LykkeJob.Contract;
using Lykke.Job.LykkeJob.Core.Services;
using Lykke.Job.LykkeJob.Core.Settings.JobSettings;
using Lykke.Job.LykkeJob.Services;
using Lykke.Service.ExchangeConnector.Client;
using Lykke.SettingsReader;
using Lykke.Job.LykkeJob.Contract;
using Lykke.RabbitMq.Azure;
using AzureStorage.Blob;
using Lykke.Job.ExchangePolling.PeriodicalHandlers;
using Lykke.Job.ExchangePolling.RabbitPublishers;
using Lykke.Job.ExchangePolling.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;

namespace Lykke.Job.LykkeJob.Modules
{
    public class JobModule : Module
    {
        private readonly ExchangePollingJobSettings _settings;
        private readonly IReloadingManager<DbSettings> _dbSettingsManager;

        private readonly ILog _log;

        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public JobModule(ExchangePollingJobSettings settings, IReloadingManager<DbSettings> dbSettingsManager, ILog log)
        {
            _settings = settings;
            _log = log;
            _dbSettingsManager = dbSettingsManager;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // NOTE: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            // builder.RegisterType<QuotesPublisher>()
            //  .As<IQuotesPublisher>()
            //  .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString))

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

            RegisterRabbitMqPublishers(builder);

            builder.RegisterType<ExchangePollingService>()
                .As<IExchangePollingService>()
                .SingleInstance();

            builder.RegisterType<ExchangeCache>()
                .As<IExchangeCache>()
                .SingleInstance();
            
            builder.RegisterType<ExchangeConnectorService>()
                .As<IExchangeConnectorService>()
                .WithParameter("baseUri", new Uri(_settings.Services.ExchangeConnectorService.Url))
                .WithParameter("credentials", new TokenCredentials(_settings.Services.ExchangeConnectorService.ApiKey))
                .SingleInstance();
            
            builder.Populate(_services);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            // TODO: You should register each periodical handler in DI container as IStartable singleton and autoactivate it

            builder.RegisterType<JfdPollingHandler>()
                .WithParameter(TypedParameter.From(_settings.BitmexSettings.PollingPeriodMilliseconds))
                .SingleInstance();
        }

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            // TODO: You should register each publisher in DI container as publisher specific interface and as IStartable,
            // as singleton and do not autoactivate it

            builder.RegisterType<RabbitMqPublisher<ExecutionReport>>()
                .As<IRabbitMqPublisher<ExecutionReport>>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameters(new[]
                {
                    new NamedParameter("connectionString", _settings.Rabbit.ExchangeConnectorOrder.ConnectionString),
                    new NamedParameter("exchangeName", _settings.Rabbit.ExchangeConnectorOrder.ExchangeName),
                    new NamedParameter("enabled", true),
                    new NamedParameter("log", _log)
                });
        }
    }
}

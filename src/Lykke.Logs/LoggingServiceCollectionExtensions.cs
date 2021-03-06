﻿using System;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeAzureTable;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Logs.Loggers.LykkeSanitizing;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Logs
{
    /// <summary>
    /// Extension methods to register Lykke logging in the app services
    /// </summary>
    [PublicAPI]
    public static class LoggingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Lykke logging services to the specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add services to.</param>
        /// <param name="configure">The <see cref="T:Microsoft.Extensions.Logging.ILogBuilder" /> configuration delegate.</param>
        /// <param name="azureTableConnectionString">Connection string reloading manager for Azure Table logger</param>
        /// <param name="azureTableName">Table name for the Azure Table logger</param>
        /// <param name="slackAzureQueueConnectionString">Connection string for the Slack loggers and health notifier</param>
        /// <param name="slackAzureQueuesBaseName">Base queue name for the Slack loggers</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that additional calls can be chained.</returns>
        [NotNull]
        public static IServiceCollection AddLykkeLogging(
            [NotNull] this IServiceCollection services,
            [NotNull] IReloadingManager<string> azureTableConnectionString,
            [NotNull] string azureTableName,
            [NotNull] string slackAzureQueueConnectionString,
            [NotNull] string slackAzureQueuesBaseName,
            [CanBeNull] Action<ILogBuilder> configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (AppEnvironment.EnvInfo == null)
            {
                throw new InvalidOperationException("ENV_INFO environment should be not empty. If you run application in your local machine, please fill up ENV_INFO with your name.");
            }
            if (AppEnvironment.Name == null)
            {
                throw new InvalidOperationException("Application name should be not empty");
            }
            if (AppEnvironment.Version == null)
            {
                throw new InvalidOperationException("Application version should be not empty");
            }

            services.AddSingleton<IHealthNotifier, HealthNotifier>(s =>
            {
                var slackSenderFactory = new HealthNotifierSlackSenderFactory();

                return new HealthNotifier(
                    slackSenderFactory.Create(slackAzureQueueConnectionString, slackAzureQueuesBaseName));
            });

            services.AddSingleton<Func<IHealthNotifier>>(s => s.GetRequiredService<IHealthNotifier>);

            services.AddSingleton<ILogFactory, LogFactory>(s => new LogFactory(
                s.GetRequiredService<ILoggerFactory>(),
                s.GetRequiredService<Func<IHealthNotifier>>(),
                s.GetService<IOptions<SanitizingOptions>>()));

            services.AddLogging();

            var builder = new LogBuilder(services);

            builder.AddWellKnownFilters();

            configure?.Invoke(builder);

            builder
                .AddConsole(builder.ConfigureConsole)
                .AddAzureTable(azureTableConnectionString, azureTableName, builder.ConfigureAzureTable)
                .AddEssentialSlackChannels(slackAzureQueueConnectionString, slackAzureQueuesBaseName, builder.ConfigureEssentialSlackChannels);

            return services;
        }

        /// <summary>
        /// Adds empty Lykke logging services to the specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that additional calls can be chained.</returns>
        [NotNull]
        public static IServiceCollection AddEmptyLykkeLogging([NotNull] this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IHealthNotifier>(s => EmptyHealthNotifier.Instance);
            services.AddSingleton<ILogFactory>(s => EmptyLogFactory.Instance);

            return services;
        }

        /// <summary>
        /// Adds console Lykke logging services to the specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add services to.</param>
        /// <param name="configure">The <see cref="T:Microsoft.Extensions.Logging.ILogBuilder" /> configuration delegate.</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that additional calls can be chained.</returns>
        [NotNull]
        public static IServiceCollection AddConsoleLykkeLogging([NotNull] this IServiceCollection services, [CanBeNull] Action<ILogBuilder> configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IHealthNotifier>(s => EmptyHealthNotifier.Instance);

            services.AddSingleton<ILogFactory, LogFactory>(s => new LogFactory(
                s.GetRequiredService<ILoggerFactory>(),
                () => EmptyHealthNotifier.Instance,
                s.GetService<IOptions<SanitizingOptions>>()));

            services.AddLogging();

            var builder = new LogBuilder(services);

            builder.AddWellKnownFilters();

            configure?.Invoke(builder);

            builder.AddConsole(builder.ConfigureConsole);

            return services;
        }

        private static void AddWellKnownFilters(this LogBuilder builder)
        {
            builder
                .AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning)
                .AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning)
                .AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", Microsoft.Extensions.Logging.LogLevel.Error);
        }
    }
}
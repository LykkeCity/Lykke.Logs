using System;
using JetBrains.Annotations;
using Lykke.Logs.Loggers.LykkeAzureTable;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    internal static class InternalLogBuilderExtensions
    {
        public static ILogBuilder AddConsole(
            [NotNull] this ILogBuilder builder,
            [CanBeNull] Action<ConsoleLoggerOptions> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new ConsoleLoggerOptions();

            configure?.Invoke(options);

            builder.Services.AddSingleton<ILoggerProvider, LykkeConsoleLoggerProvider>(s =>
                new LykkeConsoleLoggerProvider(options, ConsoleLogMessageWriter.Instance));

            return builder;
        }

        public static ILogBuilder AddAzureTable(
            [NotNull] this ILogBuilder builder,
            [NotNull] IReloadingManager<string> connectionString,
            [NotNull] string tableName,
            [CanBeNull] Action<AzureTableLoggerOptions> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new AzureTableLoggerOptions();

            configure?.Invoke(options);

            builder.Services.AddSingleton<ILoggerProvider, LykkeAzureTableLoggerProvider>(s => new LykkeAzureTableLoggerProvider(
                connectionString,
                tableName,
                options));

            return builder;
        }

        public static ILogBuilder AddEssentialSlackChannels(
            [NotNull] this ILogBuilder builder,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName,
            [CanBeNull] Action<SlackLoggerOptions> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrWhiteSpace(azureQueueConnectionString))
            {
                throw new ArgumentNullException(nameof(azureQueueConnectionString));
            }
            if (string.IsNullOrWhiteSpace(azureQueuesBaseName))
            {
                throw new ArgumentNullException();
            }

            var spamGuard = new SpamGuard<Microsoft.Extensions.Logging.LogLevel>(DirectConsoleLogFactory.Instance);

            foreach (var level in new[]
            {
                Microsoft.Extensions.Logging.LogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error
            })
            {
                spamGuard.SetMutePeriod(level, TimeSpan.FromMinutes(1));
            }

            spamGuard.Start();

            var options = new SlackLoggerOptions(spamGuard);

            configure?.Invoke(options);

            // This will be used by additional Slack channel loggers
            var generalOptions = new GeneralSlackLoggerOptions(azureQueueConnectionString, azureQueuesBaseName);
            
            builder.Services.AddSingleton(generalOptions);

            builder.Services.AddSingleton<ILoggerProvider, LykkeSlackLoggerProvider>(s => new LykkeSlackLoggerProvider(
                azureQueueConnectionString,
                azureQueuesBaseName,
                spamGuard,
                level =>
                {
                    switch (level)
                    {
                        case Microsoft.Extensions.Logging.LogLevel.Trace:
                        case Microsoft.Extensions.Logging.LogLevel.Debug:
                        case Microsoft.Extensions.Logging.LogLevel.Information:
                            return null;

                        case Microsoft.Extensions.Logging.LogLevel.Warning:
                            return "Warning";
                        case Microsoft.Extensions.Logging.LogLevel.Error:
                        case Microsoft.Extensions.Logging.LogLevel.Critical:
                            return "Errors";

                        default:
                            throw new ArgumentOutOfRangeException(nameof(level), level, null);
                    }
                }));

            return builder;
        }
    }
}
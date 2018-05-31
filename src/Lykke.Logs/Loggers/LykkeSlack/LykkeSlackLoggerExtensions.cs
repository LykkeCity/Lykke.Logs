using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    [PublicAPI]
    public static class LykkeSlackLoggerExtensions
    {
        /// <summary>
        /// Adds a Slack logger that logs entries to the essential Lykke Slack channels.
        /// This logger will logs <see cref="Microsoft.Extensions.Logging.LogLevel.Warning"/>,
        /// <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/> and
        /// <see cref="Microsoft.Extensions.Logging.LogLevel.Critical"/> entries to the sys-warnings
        /// and app-errors channels
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="azureQueueConnectionString">Azure Storage connection string</param>
        /// <param name="azureQueuesBaseName">Base name for the Azure Storage queues</param>
        /// <param name="configure">Optional configuration action</param>
        public static ILoggingBuilder AddLykkeEssentialSlackChannels(
            [NotNull] this ILoggingBuilder builder,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName,
            [CanBeNull] Action<ISpamGuardConfiguration<Microsoft.Extensions.Logging.LogLevel>> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var spamGuard = new SpamGuard<Microsoft.Extensions.Logging.LogLevel>(LastResortLogFactory.Instance);

            foreach (var level in new[]
            {
                Microsoft.Extensions.Logging.LogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error
            })
            {
                spamGuard.SetMutePeriod(level, TimeSpan.FromMinutes(1));
            }

            configure?.Invoke(spamGuard);

            spamGuard.Start();

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

        /// <summary>
        /// Adds a Slack logger that logs entries to the additional Slack channel.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="azureQueueConnectionString">Azure Storage connection string</param>
        /// <param name="azureQueuesBaseName">Base name for the Azure Storage queues</param>
        /// <param name="channel">Channel ID</param>
        /// <param name="minLevel">Minimal logging level</param>
        /// <param name="configure">Optional configuration action</param>
        public static ILoggingBuilder AddLykkeAdditionalSlackChannel(
            [NotNull] this ILoggingBuilder builder,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName,
            [NotNull] string channel,
            Microsoft.Extensions.Logging.LogLevel minLevel = Microsoft.Extensions.Logging.LogLevel.Information,
            [CanBeNull] Action<ISpamGuardConfiguration<Microsoft.Extensions.Logging.LogLevel>> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            var spamGuard = new SpamGuard<Microsoft.Extensions.Logging.LogLevel>(LastResortLogFactory.Instance);

            foreach (var level in new []
            {
                Microsoft.Extensions.Logging.LogLevel.Information,
                Microsoft.Extensions.Logging.LogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error
            })
            {
                spamGuard.SetMutePeriod(level, TimeSpan.FromMinutes(1));
            }

            configure?.Invoke(spamGuard);

            spamGuard.Start();

            builder.Services.AddSingleton<ILoggerProvider, LykkeSlackLoggerProvider>(s => new LykkeSlackLoggerProvider(
                azureQueueConnectionString,
                azureQueuesBaseName, 
                spamGuard,
                level => level >= minLevel ? channel : null));

            return builder;
        }
    }
}
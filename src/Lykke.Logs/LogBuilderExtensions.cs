using System;
using JetBrains.Annotations;
using Lykke.Logs.Loggers.LykkeSlack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    [PublicAPI]
    public static class LogBuilderExtensions
    {      

        /// <summary>
        /// Adds a Slack logger that logs entries to the additional Slack channel.
        /// </summary>
        /// <param name="builder">The <see cref="ILogBuilder"/> to use.</param>
        /// <param name="azureQueueConnectionString">Azure Storage connection string</param>
        /// <param name="azureQueuesBaseName">Base name for the Azure Storage queues</param>
        /// <param name="channel">Channel ID</param>
        /// <param name="configure">Optional configuration action</param>
        public static ILogBuilder AddAdditionalSlackChannel(
            [NotNull] this ILogBuilder builder,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName,
            [NotNull] string channel,
            [CanBeNull] Action<AdditionalSlackLoggerOptions> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            var spamGuard = new SpamGuard<Microsoft.Extensions.Logging.LogLevel>(DirectConsoleLogFactory.Instance);

            foreach (var level in new[]
            {
                Microsoft.Extensions.Logging.LogLevel.Information,
                Microsoft.Extensions.Logging.LogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error
            })
            {
                spamGuard.SetMutePeriod(level, TimeSpan.FromMinutes(1));
            }

            spamGuard.Start();

            var options = new AdditionalSlackLoggerOptions(spamGuard);

            configure?.Invoke(options);

            builder.Services.AddSingleton<ILoggerProvider, LykkeSlackLoggerProvider>(s => new LykkeSlackLoggerProvider(
                azureQueueConnectionString,
                azureQueuesBaseName, 
                spamGuard,
                level => level >= options.MinLogLevel ? channel : null));

            return builder;
        }
    }
}
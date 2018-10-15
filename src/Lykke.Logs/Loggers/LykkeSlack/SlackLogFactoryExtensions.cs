using System;
using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    [PublicAPI]
    public static class SlackLogFactoryExtensions
    {
        /// <summary>
        /// Adds a Slack logger that logs to the essential Slack channels
        /// </summary>
        /// <param name="factory">The <see cref="ILogFactory"/> to use.</param>
        /// <param name="azureQueueConnectionString">Azure storage queue connection string</param>
        /// <param name="azureQueuesBaseName">Azure queue base name</param>
        /// <param name="configure">Optional configuration</param>
        public static ILogFactory AddEssentialSlackChannels(
            [NotNull] this ILogFactory factory,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName,
            Action<SlackLoggerOptions> configure = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if (string.IsNullOrWhiteSpace(azureQueueConnectionString))
            {
                throw new ArgumentNullException(nameof(azureQueueConnectionString));
            }
            if (string.IsNullOrWhiteSpace(azureQueuesBaseName))
            {
                throw new ArgumentNullException();
            }

            var spamGuard = SlackSpamGuardBuilder.BuildForEssentialSlackChannelsSpamGuard();
            var options = new SlackLoggerOptions(spamGuard);

            configure?.Invoke(options);

            factory.AddProvider(new SlackLoggerProvider(
                azureQueueConnectionString,
                azureQueuesBaseName,
                spamGuard,
                SlackChannelResolvers.EssentialChannelsResolver,
                options.IsChaosExceptionFilteringEnabled));

            return factory;
        }

        /// <summary>
        /// Adds a Slack logger that logs entries to the additional Slack channel.
        /// </summary>
        /// <param name="factory">The <see cref="ILogBuilder"/> to use.</param>
        /// <param name="azureQueueConnectionString">Azure storage queue connection string</param>
        /// <param name="azureQueuesBaseName">Azure queue base name</param>
        /// <param name="channel">Channel ID</param>
        /// <param name="configure">Optional configuration action</param>
        public static ILogFactory AddAdditionalSlackChannel(
            [NotNull] this ILogFactory factory,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName,
            [NotNull] string channel,
            [CanBeNull] Action<AdditionalSlackLoggerOptions> configure = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if (string.IsNullOrWhiteSpace(azureQueueConnectionString))
            {
                throw new ArgumentNullException(nameof(azureQueueConnectionString));
            }
            if (string.IsNullOrWhiteSpace(azureQueuesBaseName))
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            var spamGuard = SlackSpamGuardBuilder.BuildForAdditionalSlackChannel();
            var options = new AdditionalSlackLoggerOptions(spamGuard);
            options.DisableChaosExceptionFiltering();

            configure?.Invoke(options);

            factory.AddProvider(new SlackLoggerProvider(
                azureQueueConnectionString,
                azureQueuesBaseName,
                spamGuard,
                SlackChannelResolvers.GetAdditionalChannelResolver(options.MinLogLevel, channel),
                options.IsChaosExceptionFilteringEnabled)
            );

            return factory;
        }
    }
}
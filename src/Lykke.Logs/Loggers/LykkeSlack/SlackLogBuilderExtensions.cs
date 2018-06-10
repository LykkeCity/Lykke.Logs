﻿using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    [PublicAPI]
    public static class SlackLogBuilderExtensions
    {
        internal static ILogBuilder AddEssentialSlackChannels(
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

            var spamGuard = SlackSpamGuardBuilder.BuildForEssentialSlackChannelsSpamGuard();
            var options = new SlackLoggerOptions(spamGuard);

            configure?.Invoke(options);

            // This will be used by additional Slack channel loggers
            var generalOptions = new GeneralSlackLoggerOptions(azureQueueConnectionString, azureQueuesBaseName);

            builder.Services.AddSingleton(generalOptions);

            builder.Services.AddSingleton<ILoggerProvider, SlackLoggerProvider>(s => new SlackLoggerProvider(
                azureQueueConnectionString,
                azureQueuesBaseName,
                spamGuard,
                SlackChannelResolvers.EssentialChannelsResolver));

            return builder;
        }

        /// <summary>
        /// Adds a Slack logger that logs entries to the additional Slack channel.
        /// </summary>
        /// <param name="builder">The <see cref="ILogBuilder"/> to use.</param>
        /// <param name="channel">Channel ID</param>
        /// <param name="configure">Optional configuration action</param>
        public static ILogBuilder AddAdditionalSlackChannel(
            [NotNull] this ILogBuilder builder,
            [NotNull] string channel,
            [CanBeNull] Action<AdditionalSlackLoggerOptions> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            var spamGuard = SlackSpamGuardBuilder.BuildForAdditionalSlackChannel();
            var options = new AdditionalSlackLoggerOptions(spamGuard);

            configure?.Invoke(options);

            builder.Services.AddSingleton<ILoggerProvider, SlackLoggerProvider>(s =>
            {
                var generalOptions = s.GetRequiredService<GeneralSlackLoggerOptions>();

                return new SlackLoggerProvider(
                    generalOptions.ConnectionString,
                    generalOptions.BaseQueuesName,
                    spamGuard,
                    SlackChannelResolvers.GetAdditionalChannelResolver(options.MinLogLevel, channel));
            });

            return builder;
        }
    }
}
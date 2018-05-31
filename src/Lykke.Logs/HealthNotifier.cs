﻿using System;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.AzureQueueIntegration;
using Lykke.AzureQueueIntegration.Publisher;
using Lykke.Common.Log;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    /// <summary>
    /// App health notifier
    /// </summary>
    [PublicAPI]
    public class HealthNotifier : IHealthNotifier
    {
        [NotNull] private readonly string _appName;
        [NotNull] private readonly string _appVersion;
        [NotNull] private readonly string _envInfo;
        [NotNull] private readonly ISlackNotificationsSender _slackSender;
        [NotNull] private readonly ILog _log;

        /// <summary>
        /// Creates lykke health notifier for the specific app
        /// </summary>
        /// <param name="appName">Name of the app</param>
        /// <param name="appVersion">Version of the app</param>
        /// <param name="envInfo">ENV_INFO environment variable of the app</param>
        /// <param name="logFactory">Log factory</param>
        /// <param name="azureQueueConnectionString">Azure Storage connection string</param>
        /// <param name="azureQueuesBaseName">Base name for the Azure Storage queues</param>
        public HealthNotifier(
            [NotNull] string appName,
            [NotNull] string appVersion,
            [NotNull] string envInfo,
            [NotNull] ILogFactory logFactory,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName)

            : this(appName, appVersion, envInfo, logFactory, CreateSlackSender(azureQueueConnectionString, azureQueuesBaseName))
        {
        }

        /// <summary>
        /// Creates lykke health notifier for the specific app
        /// </summary>
        /// <param name="appName">Name of the app</param>
        /// <param name="appVersion">Version of the app</param>
        /// <param name="envInfo">ENV_INFO environment variable of the app</param>
        /// <param name="logFactory">Log factory</param>
        /// <param name="slackSender">Slack sender</param>
        private HealthNotifier(
            [NotNull] string appName,
            [NotNull] string appVersion,
            [NotNull] string envInfo,
            [NotNull] ILogFactory logFactory,
            [NotNull] ISlackNotificationsSender slackSender)
        {
            if (logFactory == null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            _appName = appName ?? throw new ArgumentNullException(nameof(appName));
            _appVersion = appVersion ?? throw new ArgumentNullException(nameof(appVersion));
            _envInfo = envInfo ?? throw new ArgumentNullException(nameof(envInfo));
            _slackSender = slackSender ?? throw new ArgumentNullException(nameof(slackSender));
            _log = logFactory.CreateLog(this);
        }

        /// <inheritdoc />
        public async Task NotifyAsync([NotNull] string healthMessage, [CanBeNull] object context)
        {
            var sender = $":loudspeaker: {_appName} {_appVersion} : {_envInfo}";

            var messageBuilder = new StringBuilder();

            messageBuilder.Append(healthMessage);

            if (context != null)
            {
                messageBuilder.AppendLine();
                messageBuilder.Append(LogContextConversion.ConvertToString(context));
            }

            var sendTask = _slackSender.SendMonitorAsync(messageBuilder.ToString(), sender);

            _log.Info(healthMessage, context);

            await sendTask;
        }

        private static ISlackNotificationsSender CreateSlackSender(string azureQueueConnectionString, string azureQueuesBaseName)
        {
            var azureQueuePublisher = new AzureQueuePublisher<SlackMessageQueueEntity>(
                    LastResortLogFactory.Instance,
                    new SlackNotificationsSerializer(),
                    "Health notifier",
                    new AzureQueueSettings
                    {
                        ConnectionString = azureQueueConnectionString,
                        QueueName = $"{azureQueuesBaseName}-monitor"
                    })
                .Start();

            return new SlackNotificationsSender(azureQueuePublisher, ownQueue: true);
        }

        public void Dispose()
        {
            (_slackSender as IDisposable)?.Dispose();
        }
    }
}
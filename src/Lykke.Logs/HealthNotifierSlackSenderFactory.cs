using System;
using JetBrains.Annotations;
using Lykke.AzureQueueIntegration;
using Lykke.AzureQueueIntegration.Publisher;
using Lykke.Common.Log;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class HealthNotifierSlackSenderFactory : IHealthNotifierSlackSenderFactory
    {
        [NotNull] private readonly ILogFactory _logFactory;

        /// <summary>
        /// Creates <see cref="HealthNotifierSlackSenderFactory"/>
        /// </summary>
        /// <param name="logFactory">Log factory</param>
        public HealthNotifierSlackSenderFactory([NotNull] ILogFactory logFactory)
        {
            _logFactory = logFactory;
        }

        /// <inheritdoc />
        public ISlackNotificationsSender Create(string azureQueueConnectionString, string azureQueuesBaseName)
        {
            if (string.IsNullOrWhiteSpace(azureQueueConnectionString))
            {
                throw new ArgumentNullException(nameof(azureQueueConnectionString));
            }
            if (string.IsNullOrWhiteSpace(azureQueuesBaseName))
            {
                throw new ArgumentNullException(nameof(azureQueuesBaseName));
            }

            var azureQueuePublisher = new AzureQueuePublisher<SlackMessageQueueEntity>(
                    _logFactory,
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
    }
}
using System;
using JetBrains.Annotations;
using Lykke.AzureQueueIntegration;
using Lykke.AzureQueueIntegration.Publisher;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class HealthNotifierSlackSenderFactory : IHealthNotifierSlackSenderFactory
    {
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
                    LogFactory.LastResort,
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

        /// <inheritdoc />
        public ISlackNotificationsSender CreateForCustomChannel(string azureQueueConnectionString, string azureQueueForCustomChannel)
        {
            if (string.IsNullOrWhiteSpace(azureQueueConnectionString))
                throw new ArgumentNullException(nameof(azureQueueConnectionString));
            if (string.IsNullOrWhiteSpace(azureQueueForCustomChannel))
                throw new ArgumentNullException(nameof(azureQueueForCustomChannel));

            var azureQueuePublisher = new AzureQueuePublisher<SlackMessageQueueEntity>(
                    LogFactory.LastResort,
                    new SlackNotificationsSerializer(),
                    "Health notifier for custom slack channel",
                    new AzureQueueSettings
                    {
                        ConnectionString = azureQueueConnectionString,
                        QueueName = azureQueueForCustomChannel
                    })
                .Start();

            return new SlackNotificationsSender(azureQueuePublisher, ownQueue: true);
        }
    }
}
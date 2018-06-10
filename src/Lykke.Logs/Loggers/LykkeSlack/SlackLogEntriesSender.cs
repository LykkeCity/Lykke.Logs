using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.AzureQueueIntegration;
using Lykke.AzureQueueIntegration.Publisher;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    internal sealed class SlackLogEntriesSender : ISlackLogEntriesSender
    {
        private readonly IReadOnlyDictionary<Microsoft.Extensions.Logging.LogLevel, ISlackNotificationsSender> _senders;

        public SlackLogEntriesSender(
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName)
        {
            if (azureQueuesBaseName == null)
            {
                throw new ArgumentNullException(nameof(azureQueuesBaseName));
            }
            
            var senders = new Dictionary<Microsoft.Extensions.Logging.LogLevel, ISlackNotificationsSender>();
            var serializer = new SlackNotificationsSerializer();

            foreach (var level in LogLevels.All)
            {
                var azureQueuePublisher = new AzureQueuePublisher<SlackMessageQueueEntity>(
                        LogFactory.LastResort,
                        serializer,
                        $"Slack log [{level}]",
                        new AzureQueueSettings
                        {
                            ConnectionString = azureQueueConnectionString,
                            QueueName = $"{azureQueuesBaseName}-{level}"
                        })
                    .Start();

                var sender = new SlackNotificationsSender(azureQueuePublisher, ownQueue: true);

                senders.Add(level, sender);
            }

            _senders = new ReadOnlyDictionary<Microsoft.Extensions.Logging.LogLevel, ISlackNotificationsSender>(senders);
        }

        public Task SendAsync(Microsoft.Extensions.Logging.LogLevel level, DateTime moment, string channel, string sender, string message)
        {
            return _senders[level].SendAsync(moment, channel, sender, message);
        }

        public void Dispose()
        {
            foreach (var sender in _senders.Values)
            {
                (sender as IDisposable)?.Dispose();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Common;
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

            var initLogsTasks = LogLevels.All.SelectAsync(p =>
                InitSender(p, serializer, azureQueueConnectionString, azureQueuesBaseName));

            Task.WhenAll(initLogsTasks);

            foreach (var levelSenderTuple in initLogsTasks.Result)
            {
                senders.Add(levelSenderTuple.logLevel, levelSenderTuple.sender);
            }

            _senders = new ReadOnlyDictionary<Microsoft.Extensions.Logging.LogLevel, ISlackNotificationsSender>(senders);
        }

        private async Task<(Microsoft.Extensions.Logging.LogLevel logLevel, ISlackNotificationsSender sender)> InitSender(
            Microsoft.Extensions.Logging.LogLevel level,
            SlackNotificationsSerializer serializer,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName)
        {
            return await Task.Run(() =>
            {
                var azureQueuePublisher = new AzureQueuePublisher<SlackMessageQueueEntity>(
                    LogFactory.LastResort,
                    serializer,
                    $"Slack log [{level}]",
                    new AzureQueueSettings
                    {
                        ConnectionString = azureQueueConnectionString,
                        QueueName = $"{azureQueuesBaseName}-{level}"
                    }, 
                    fireNForgetQueueExistenceCheck: true)
                 .Start();

                var sender = new SlackNotificationsSender(azureQueuePublisher, ownQueue: true);

                return (level, sender);
            });
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
using JetBrains.Annotations;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    /// <summary>
    /// Factory that creates <see cref="ISlackNotificationsSender"/> for the <see cref="HealthNotifier"/>
    /// </summary>
    [PublicAPI]
    public interface IHealthNotifierSlackSenderFactory
    {
        /// <summary>
        /// Creates <see cref="ISlackNotificationsSender"/> for the <see cref="HealthNotifier"/>
        /// </summary>
        /// <param name="azureQueueConnectionString">Azure storage connection string, where the slack notifications queue is</param>
        /// <param name="azureQueuesBaseName">Slack notifications queues base name</param>
        /// <returns>ISlackNotificationsSender instance.</returns>
        [NotNull]
        ISlackNotificationsSender Create(
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName);

        /// <summary>
        /// Creates <see cref="ISlackNotificationsSender"/> for the <see cref="HealthNotifier"/> to send mesages into custom slack channel.
        /// </summary>
        /// <param name="azureQueueConnectionString">Azure storage connection string, where the slack notifications queue is</param>
        /// <param name="azureQueueForCustomChannel">Slack notifications queue name that is used for custom channel.</param>
        /// <returns>ISlackNotificationsSender instance.</returns>
        [NotNull]
        ISlackNotificationsSender CreateForCustomChannel([NotNull] string azureQueueConnectionString, [NotNull] string azureQueueForCustomChannel);
    }
}
using System;
using System.Text;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    /// <summary>
    /// App health notifier
    /// </summary>
    [PublicAPI]
    public sealed class HealthNotifier : IHealthNotifier
    {
        [NotNull] private readonly ILogFactory _logFactory;
        [NotNull] private readonly string _appName;
        [NotNull] private readonly string _appVersion;
        [NotNull] private readonly string _envInfo;
        [NotNull] private readonly ISlackNotificationsSender _slackSender;

        /// <summary>
        /// Creates lykke health notifier for the specific app
        /// </summary>
        /// <param name="logFactory">Log factory</param>
        /// <param name="slackSenderFactory">Slack sender factory</param>
        /// <param name="azureQueueConnectionString">Azure Storage connection string</param>
        /// <param name="azureQueuesBaseName">Base name for the Azure Storage queues</param>
        internal HealthNotifier(
            [NotNull] ILogFactory logFactory,
            [NotNull] IHealthNotifierSlackSenderFactory slackSenderFactory,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName)

            : this(
                  AppEnvironment.Name, 
                  AppEnvironment.Version, 
                  AppEnvironment.EnvInfo, 
                  logFactory, 
                  slackSenderFactory,
                  azureQueueConnectionString,
                  azureQueuesBaseName)
        {
        }

        /// <summary>
        /// Creates lykke health notifier for the specific app
        /// </summary>
        /// <param name="appName">Name of the app</param>
        /// <param name="appVersion">Version of the app</param>
        /// <param name="envInfo">ENV_INFO environment variable of the app</param>
        /// <param name="logFactory">Log factory</param>
        /// <param name="slackSenderFactory">Slack sender factory</param>
        /// <param name="azureQueueConnectionString">Azure Storage connection string</param>
        /// <param name="azureQueuesBaseName">Base name for the Azure Storage queues</param>
        public HealthNotifier(
            [NotNull] string appName, 
            [NotNull] string appVersion, 
            [NotNull] string envInfo,
            [NotNull] ILogFactory logFactory, 
            [NotNull] IHealthNotifierSlackSenderFactory slackSenderFactory,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName)
        {
            _appName = appName ?? throw new ArgumentNullException(nameof(appName));
            _appVersion = appVersion ?? throw new ArgumentNullException(nameof(appVersion));
            _envInfo = envInfo ?? throw new ArgumentNullException(nameof(envInfo));
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));

            if (slackSenderFactory == null)
            {
                throw new ArgumentNullException(nameof(slackSenderFactory));
            }

            _slackSender = slackSenderFactory.Create(azureQueueConnectionString, azureQueuesBaseName);

            if (_slackSender == null)
            {
                throw new InvalidOperationException("Slack sender factory created no Slack sender");
            }
        }

        /// <inheritdoc />
        public void Notify([NotNull] string healthMessage, [CanBeNull] object context)
        {
            var sender = $":loudspeaker: {_appName} {_appVersion} : {_envInfo}";

            var messageBuilder = new StringBuilder();

            messageBuilder.Append(healthMessage);

            if (context != null)
            {
                messageBuilder.AppendLine();
                messageBuilder.Append(LogContextConversion.ConvertToString(context));
            }

            // TODO: Actually there is no IO, so ISlackNotificationsSender should be refactored to be synchronous
            _slackSender.SendMonitorAsync(messageBuilder.ToString(), sender).ConfigureAwait(false).GetAwaiter().GetResult();

            _logFactory.CreateLog(this).Info(healthMessage, context);
        }

        public void Dispose()
        {
            (_slackSender as IDisposable)?.Dispose();
        }
    }
}
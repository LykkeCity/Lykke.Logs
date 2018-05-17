using System.Text;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
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
        /// <param name="slackSender">Slack sender</param>
        public HealthNotifier(
            [NotNull] string appName,
            [NotNull] string appVersion,
            [NotNull] string envInfo,
            [NotNull] ILogFactory logFactory,
            [NotNull] ISlackNotificationsSender slackSender)
        {
            _appName = appName;
            _appVersion = appVersion;
            _envInfo = envInfo;
            _slackSender = slackSender;
            _log = logFactory.CreateLog(this);
        }

        /// <inheritdoc />
        public async Task NotifyAsync([NotNull] string healthMessage, [CanBeNull] object context)
        {
            var messageBuilder = new StringBuilder();

            messageBuilder.Append($"{_appName} {_appVersion} : {_envInfo} : {healthMessage}");

            if (context != null)
            {
                messageBuilder.AppendLine();
                messageBuilder.Append(LogContextConversion.ConvertToString(context));
            }

            var sendTask = _slackSender.SendMonitorAsync(messageBuilder.ToString());

            _log.Info(healthMessage, context);

            await sendTask;
        }
    }
}
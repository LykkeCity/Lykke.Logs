using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [NotNull] private readonly string _appName;
        [NotNull] private readonly string _appVersion;
        [NotNull] private readonly string _envInfo;
        [NotNull] private readonly ISlackNotificationsSender _slackSender;
        [NotNull] private readonly HashSet<string> _customChannels = new HashSet<string>();

        /// <summary>
        /// Creates lykke health notifier for the specific app
        /// </summary>
        /// <param name="slackSender">Slack sender factory</param>
        internal HealthNotifier([NotNull] ISlackNotificationsSender slackSender)

            : this(
                  AppEnvironment.Name, 
                  AppEnvironment.Version, 
                  AppEnvironment.EnvInfo, 
                  slackSender)
        {
        }

        /// <summary>
        /// Creates lykke health notifier for the specific app
        /// </summary>
        /// <param name="appName">Name of the app</param>
        /// <param name="appVersion">Version of the app</param>
        /// <param name="envInfo">ENV_INFO environment variable of the app</param>
        /// <param name="slackSender">Slack sender</param>
        public HealthNotifier(
            [NotNull] string appName, 
            [NotNull] string appVersion, 
            [NotNull] string envInfo,
            [NotNull] ISlackNotificationsSender slackSender)
        {
            _appName = appName ?? throw new ArgumentNullException(nameof(appName));
            _appVersion = appVersion ?? throw new ArgumentNullException(nameof(appVersion));
            _envInfo = envInfo ?? throw new ArgumentNullException(nameof(envInfo));
            _slackSender = slackSender ?? throw new ArgumentNullException(nameof(slackSender));
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

            var message = messageBuilder.ToString();
            var tasks = new List<Task> { _slackSender.SendMonitorAsync(message, sender) };
            tasks.AddRange(_customChannels.Select(c => _slackSender.SendAsync(c, sender, message)));

            Task.WhenAll(tasks).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            (_slackSender as IDisposable)?.Dispose();
        }

        internal void AddCustomSlackSender(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentNullException();

            _customChannels.Add(channel);
        }
    }
}
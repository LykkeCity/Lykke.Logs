using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    /// <summary>
    /// Class for async sending of slack messages from internal queue.
    /// </summary>
    [Obsolete("Use new Lykke logging system")]
    [PublicAPI]
    public class LykkeLogToAzureSlackNotificationsManager : ProducerConsumer<LogEntity>, ILykkeLogToAzureSlackNotificationsManager
    {
        private readonly ISlackNotificationsSender _slackNotificationsSender;
        private readonly ILog _lastResortLog;
        private readonly string _component;
        private readonly HashSet<string> _logLevels;
        private readonly SpamGuard<LogLevel> _spamGuard;

        /// <summary>
        /// C-tor with a custom component name and default log levels collection.
        /// </summary>
        /// <param name="componentName">Custom component name</param>
        /// <param name="slackNotificationsSender">Sender for slack messages</param>
        /// <param name="lastResortLog">Logger</param>
        public LykkeLogToAzureSlackNotificationsManager(
            string componentName,
            ISlackNotificationsSender slackNotificationsSender,
            ILog lastResortLog = null)
            : this(componentName, slackNotificationsSender, false, lastResortLog)
        {
        }

        /// <summary>
        /// C-tor with standard component name and default log levels collection.
        /// </summary>
        /// <param name="slackNotificationsSender">Sender for slack messages</param>
        /// <param name="lastResortLog">Logger</param>
        public LykkeLogToAzureSlackNotificationsManager(
            ISlackNotificationsSender slackNotificationsSender,
            ILog lastResortLog = null)
            : this(AppEnvironment.Name, slackNotificationsSender, false, lastResortLog)
        {
        }

        /// <summary>
        /// C-tor with standard component name and custom collection of log levels.
        /// </summary>
        /// <param name="slackNotificationsSender"></param>
        /// <param name="logLevels"></param>
        /// <param name="lastResortLog"></param>
        public LykkeLogToAzureSlackNotificationsManager(
            ISlackNotificationsSender slackNotificationsSender,
            HashSet<string> logLevels,
            ILog lastResortLog = null)
            : this(AppEnvironment.Name, slackNotificationsSender, false, lastResortLog)
        {
            _logLevels = logLevels ?? new HashSet<string>();
        }

        /// <summary>
        /// C-tor with a standard component name and deafult log levels collection and antispam protection control flag.
        /// </summary>
        /// <param name="slackNotificationsSender">Sender for slack messages</param>
        /// <param name="disableAntiSpam">Flag for antispam protection control</param>
        /// <param name="lastResortLog">Logger</param>
        public LykkeLogToAzureSlackNotificationsManager(
            ISlackNotificationsSender slackNotificationsSender,
            bool disableAntiSpam,
            ILog lastResortLog = null)
            : this(AppEnvironment.Name, slackNotificationsSender, disableAntiSpam, lastResortLog)
        {
        }

        /// <summary>
        /// C-tor with a custom component name and deafult log levels collection and antispam protection control flag.
        /// </summary>
        /// <param name="componentName">Custom component name</param>
        /// <param name="slackNotificationsSender">Sender for slack messages</param>
        /// <param name="disableAntiSpam">Flag for antispam protection control</param>
        /// <param name="lastResortLog">Logger</param>
        public LykkeLogToAzureSlackNotificationsManager(
            string componentName,
            ISlackNotificationsSender slackNotificationsSender,
            bool disableAntiSpam,
            ILog lastResortLog = null)
            : base(componentName, lastResortLog)
        {
            _slackNotificationsSender = slackNotificationsSender;
            _lastResortLog = lastResortLog ?? new LogToConsole();
            _component = componentName;
            _logLevels = DefaultLogLevelsInit();
            _spamGuard = new SpamGuard<LogLevel>(lastResortLog ?? new LogToConsole());
            if (disableAntiSpam)
            {
                _spamGuard.DisableGuarding();
            }
            else
            {
                SetSpamMutePeriodForLevels(TimeSpan.FromMinutes(1), LogLevel.Warning, LogLevel.Error);
                _spamGuard.Start();
            }
        }

        /// <summary>
        /// Sets spam same mute period for all provided log levels.
        /// </summary>
        /// <param name="mutePeriod">Mute period for spam</param>
        /// <param name="levels">Log levels to be muted in case of spam</param>
        /// <returns>Original instance - for calls chain</returns>
        public LykkeLogToAzureSlackNotificationsManager SetSpamMutePeriodForLevels(TimeSpan mutePeriod, params LogLevel[] levels)
        {
            foreach (var level in levels)
            {
                _spamGuard.SetMutePeriod(level, mutePeriod);
            }
            return this;
        }

        /// <summary>
        /// Adds log message to internal queue.
        /// </summary>
        /// <param name="entry">Log message</param>
        public void SendNotification(LogEntity entry)
        {
            Produce(entry);
        }

        protected override async Task Consume(LogEntity entry)
        {
            try
            {
                if (!_logLevels.Contains(entry.Level))
                    return;

                var componentName = GetComponentName(entry);

                switch (entry.Level)
                {
                    case LykkeLogToAzureStorage.FatalErrorType:
                    {
                        var message = entry.Context != null
                            ? $"{entry.Msg} : {entry.Stack} : {entry.Context}"
                            : $"{entry.Msg} : {entry.Stack}";
                        await _slackNotificationsSender.SendErrorAsync(message, componentName);
                        break;
                    }

                    case LykkeLogToAzureStorage.ErrorType:
                    {
                        if (await _spamGuard.ShouldBeMutedAsync(LogLevel.Error, componentName, entry.Process))
                            break;

                        var message = entry.Context != null
                            ? $"{entry.Msg} : {entry.Stack} : {entry.Context}"
                            : $"{entry.Msg} : {entry.Stack}";
                        await _slackNotificationsSender.SendErrorAsync(message, componentName);
                        break;
                    }

                    case LykkeLogToAzureStorage.WarningType:
                    {
                        if (await _spamGuard.ShouldBeMutedAsync(LogLevel.Warning, componentName, entry.Process))
                            break;

                        var message = entry.Context != null
                            ? $"{entry.Msg} : {entry.Context}"
                            : entry.Msg;
                        await _slackNotificationsSender.SendWarningAsync(message, componentName);
                        break;
                    }

                    case LykkeLogToAzureStorage.MonitorType:
                    {
                        if (await _spamGuard.ShouldBeMutedAsync(LogLevel.Monitoring, componentName, entry.Process))
                            break;

                        var message = entry.Context != null
                            ? $"{entry.Msg} : {entry.Context}"
                            : entry.Msg;
                        await _slackNotificationsSender.SendMonitorAsync(message, componentName);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                await _lastResortLog.WriteErrorAsync("Send log entries to the Slack", "", ex);
            }
        }

        private string GetComponentName(LogEntity entry)
        {
            var sb = new StringBuilder();

            sb.Append($"{_component} {entry.Version}");

            if (!string.IsNullOrWhiteSpace(entry.Env))
                sb.Append($" : {entry.Env}");

            if (_component == null || !_component.StartsWith(entry.Component))
                sb.Append($" : {entry.Component}");

            return sb.ToString();
        }

        private HashSet<string> DefaultLogLevelsInit()
        {
            return new HashSet<string>
            {
                LykkeLogToAzureStorage.ErrorType,
                LykkeLogToAzureStorage.FatalErrorType,
                LykkeLogToAzureStorage.WarningType,
                LykkeLogToAzureStorage.MonitorType,
            };
        }
    }
}
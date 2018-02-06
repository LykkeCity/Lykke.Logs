using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    public class LykkeLogToAzureSlackNotificationsManager : ProducerConsumer<LogEntity>, ILykkeLogToAzureSlackNotificationsManager
    {
        private readonly ISlackNotificationsSender _slackNotificationsSender;
        private readonly ILog _lastResortLog;
        private readonly string _component;
        private readonly IEnumerable<string> _logLevels;

        public LykkeLogToAzureSlackNotificationsManager(
            string componentName,
            ISlackNotificationsSender slackNotificationsSender,
            ILog lastResortLog = null)
            : base(componentName, lastResortLog)
        {
            _slackNotificationsSender = slackNotificationsSender;
            _lastResortLog = lastResortLog ?? new LogToConsole();
            _component = componentName;
            _logLevels = new string[4]
            {
                LykkeLogToAzureStorage.ErrorType,
                LykkeLogToAzureStorage.FatalErrorType,
                LykkeLogToAzureStorage.WarningType,
                LykkeLogToAzureStorage.MonitorType,
            };
        }

        public LykkeLogToAzureSlackNotificationsManager(
            ISlackNotificationsSender slackNotificationsSender,
            ILog lastResortLog = null)
            : base(lastResortLog)
        {
            _slackNotificationsSender = slackNotificationsSender;
            _lastResortLog = lastResortLog ?? new LogToConsole();
            _component = AppEnvironment.Name;
            _logLevels = new string[4]
            {
                LykkeLogToAzureStorage.ErrorType,
                LykkeLogToAzureStorage.FatalErrorType,
                LykkeLogToAzureStorage.WarningType,
                LykkeLogToAzureStorage.MonitorType,
            };
        }

        public LykkeLogToAzureSlackNotificationsManager(
            ISlackNotificationsSender slackNotificationsSender,
            IEnumerable<string> logLevels,
            ILog lastResortLog = null)
            : base(lastResortLog)
        {
            _slackNotificationsSender = slackNotificationsSender;
            _lastResortLog = lastResortLog ?? new LogToConsole();
            _component = AppEnvironment.Name;
            _logLevels = logLevels ?? new string[0];
        }

        public void SendNotification(LogEntity entry)
        {
            Produce(entry);
        }

        protected override async Task Consume(LogEntity entry)
        {
            try
            {
                if (_logLevels.All(l => l != entry.Level))
                    return;

                var componentName = GetComponentName(entry);

                switch (entry.Level)
                {
                    case LykkeLogToAzureStorage.ErrorType:
                    case LykkeLogToAzureStorage.FatalErrorType:
                    {
                        var message = entry.Context != null
                            ? $"{entry.Msg} : {entry.Stack} : {entry.Context}"
                            : $"{entry.Msg} : {entry.Stack}";

                        await _slackNotificationsSender.SendErrorAsync(message, componentName);

                        break;
                    }

                    case LykkeLogToAzureStorage.WarningType:
                    {
                        var message = entry.Context != null
                            ? $"{entry.Msg} : {entry.Context}"
                            : entry.Msg;

                        await _slackNotificationsSender.SendWarningAsync(message, componentName);

                        break;
                    }

                    case LykkeLogToAzureStorage.MonitorType:
                    {
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
            {
                sb.Append($" : {entry.Env}");
            }

            if (_component == null || !_component.StartsWith(entry.Component))
            {
                sb.Append($" : {entry.Component}");
            }

            return sb.ToString();
        }
    }
}
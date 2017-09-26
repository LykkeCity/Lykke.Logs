using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;
using Common;
using Common.Log;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    public class LykkeLogToAzureSlackNotificationsManager : ProducerConsumer<LogEntity>, ILykkeLogToAzureSlackNotificationsManager
    {
        private readonly ISlackNotificationsSender _slackNotificationsSender;
        private readonly string _component;

        public LykkeLogToAzureSlackNotificationsManager(
            string componentName,
            ISlackNotificationsSender slackNotificationsSender,
            ILog lastResortLog = null)
            : base(componentName, lastResortLog)
        {
            _slackNotificationsSender = slackNotificationsSender;
            _component = componentName;
        }

        public LykkeLogToAzureSlackNotificationsManager(
            ISlackNotificationsSender slackNotificationsSender,
            ILog lastResortLog = null)
            : base(lastResortLog)
        {
            _slackNotificationsSender = slackNotificationsSender;
            var app = PlatformServices.Default.Application;
            _component = $"{app.ApplicationName} {app.ApplicationVersion}";
        }

        public void SendNotification(LogEntity entry)
        {
            Produce(entry);
        }

        protected override async Task Consume(LogEntity entry)
        {
            if (entry.Level != LykkeLogToAzureStorage.ErrorType
                && entry.Level != LykkeLogToAzureStorage.FatalErrorType
                && entry.Level != LykkeLogToAzureStorage.WarningType)
                return;

            var componentName = _component != null && _component.StartsWith(entry.Component)
                ? _component
                : $"{_component}:{entry.Component}";

            switch (entry.Level)
            {
                case LykkeLogToAzureStorage.ErrorType:
                case LykkeLogToAzureStorage.FatalErrorType:
                    await _slackNotificationsSender.SendErrorAsync($"{entry.Msg} : {entry.Stack}", componentName);
                    break;

                case LykkeLogToAzureStorage.WarningType:
                    await _slackNotificationsSender.SendWarningAsync($"{entry.Msg}", componentName);
                    break;
            }
        }
    }
}
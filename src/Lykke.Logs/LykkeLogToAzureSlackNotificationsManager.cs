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
            _component = PlatformServices.Default.Application.ApplicationName;
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
                ? $"{_component} {entry.Version}"
                : $"{_component} {entry.Version}:{entry.Component}";

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
            }
        }
    }
}
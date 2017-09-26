using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.SlackNotifications;

namespace Lykke.Logs
{
    public class LykkeLogToAzureSlackNotificationsManager : 
        ProducerConsumer<LogEntity>,
        ILykkeLogToAzureSlackNotificationsManager
    {
        private readonly ISlackNotificationsSender _slackNotificationsSender;

        public LykkeLogToAzureSlackNotificationsManager(string componentName, ISlackNotificationsSender slackNotificationsSender, ILog lastResortLog = null) : 
            base(componentName, lastResortLog)
        {
            _slackNotificationsSender = slackNotificationsSender;
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

            var componentName = _componentName != null && _componentName.StartsWith(entry.Component)
                ? _componentName
                : $"{_componentName}:{entry.Component}";

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
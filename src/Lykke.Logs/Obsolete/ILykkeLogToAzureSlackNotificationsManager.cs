using System;

namespace Lykke.Logs
{
    [Obsolete("Use new Lykke logging system")]
    public interface ILykkeLogToAzureSlackNotificationsManager
    {
        void SendNotification(LogEntity entry);
    }
}
namespace Lykke.Logs
{
    public interface ILykkeLogToAzureSlackNotificationsManager
    {
        void SendNotification(LogEntity entry);
    }
}
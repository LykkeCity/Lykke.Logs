using System;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using Lykke.SlackNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs
{
    public static class LykkeLogToAzureBinder
    {
        /// <param name="serviceCollection">Service collection to which log instance will be added</param>
        /// <param name="connectionString">Connection string's realoading manager</param>
        /// <param name="slackNotificationsSender">Slack notification sender to which warnings and errors will be forwarded</param>
        /// <param name="tableName">Log's table name. Default is "Logs"</param>
        /// <param name="lastResortLog">Last resort log (e.g. Console), which will be used to log logging infrastructure's issues</param>
        /// <param name="maxBatchLifetime">Log entries batch's lifetime, when exceeded, batch will be saved, and new batch will be started. Default is 5 seconds</param>
        /// <param name="batchSizeThreshold">Log messages batch's max size, when exceeded, batch will be saved, and new batch will be started. Default is 100 entries</param>
        /// <param name="maxRetriesCount">Max count of retries to save log entries batch into storage</param>
        /// <param name="retryDelay">Gap between retries on insert failure. Default value is 5 seconds</param>
        public static LykkeLogToAzureStorage UseLogToAzureStorage(this IServiceCollection serviceCollection,
            IReloadingManager<string> connectionString,
            ISlackNotificationsSender slackNotificationsSender = null,
            string tableName = "Logs",
            ILog lastResortLog = null,
            TimeSpan? maxBatchLifetime = null,
            int batchSizeThreshold = 100,
            int maxRetriesCount = 10,
            TimeSpan? retryDelay = null)
        {
            var applicationName = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationName;

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                applicationName,
                AzureTableStorage<LogEntity>.Create(connectionString, tableName, lastResortLog),
                lastResortLog,
                maxRetriesCount,
                retryDelay);

            var slackNotificationsManager = slackNotificationsSender != null
                ? new LykkeLogToAzureSlackNotificationsManager(applicationName, slackNotificationsSender, lastResortLog)
                : null;

            var log = new LykkeLogToAzureStorage(
                applicationName,
                persistenceManager,
                slackNotificationsManager,
                lastResortLog,
                maxBatchLifetime,
                batchSizeThreshold,
                ownPersistenceManager: true,
                ownSlackNotificationsManager: true);

            log.Start();

            serviceCollection.AddSingleton<ILog>(log);

            return log;
        }
    }
}
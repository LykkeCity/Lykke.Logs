using System;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SlackNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs
{
    public static class LykkeLogToAzureBinder
    {
        /// <param name="serviceCollection">Service collection to which log instance will be added</param>
        /// <param name="connectionString">Connection string</param>
        /// <param name="slackNotificationsSender">Slack notification sender to which warnings and errors will be forwarded</param>
        /// <param name="tableName">Log's table name. Default is "Logs"</param>
        /// <param name="lastResortLog">Last resort log (e.g. Console), which will be used to log logging infrastructure's issues</param>
        /// <param name="maxBatchLifetime">Log entries batch's lifetime, when exceeded, batch will be saved, and new batch will be started. Default is 5 seconds</param>
        /// <param name="maxBatchSize">Log messages batch's max size, when exceeded, batch will be saved, and new batch will be started. Default is 100 entries</param>
        public static LykkeLogToAzureStorage UseLogToAzureStorage(this IServiceCollection serviceCollection,
            string connectionString,
            ISlackNotificationsSender slackNotificationsSender = null,
            string tableName = "Logs",
            ILog lastResortLog = null,
            TimeSpan? maxBatchLifetime = null,
            int maxBatchSize = 100)
        {

            return UseLogToAzureStorage(serviceCollection, () => connectionString, slackNotificationsSender, tableName, lastResortLog);
        }

        /// <param name="serviceCollection">Service collection to which log instance will be added</param>
        /// <param name="getConnectionString">Connection string's factory</param>
        /// <param name="slackNotificationsSender">Slack notification sender to which warnings and errors will be forwarded</param>
        /// <param name="tableName">Log's table name. Default is "Logs"</param>
        /// <param name="lastResortLog">Last resort log (e.g. Console), which will be used to log logging infrastructure's issues</param>
        /// <param name="maxBatchLifetime">Log entries batch's lifetime, when exceeded, batch will be saved, and new batch will be started. Default is 5 seconds</param>
        /// <param name="maxBatchSize">Log messages batch's max size, when exceeded, batch will be saved, and new batch will be started. Default is 100 entries</param>
        public static LykkeLogToAzureStorage UseLogToAzureStorage(this IServiceCollection serviceCollection,
            Func<string> getConnectionString,
            ISlackNotificationsSender slackNotificationsSender = null,
            string tableName = "Logs",
            ILog lastResortLog = null,
            TimeSpan? maxBatchLifetime = null,
            int maxBatchSize = 100)
        {
            var applicationName = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationName;

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                applicationName,
                AzureTableStorage<LogEntity>.Create(getConnectionString, tableName, lastResortLog),
                lastResortLog);

            var slackNotificationsManager = slackNotificationsSender != null
                ? new LykkeLogToAzureSlackNotificationsManager(applicationName, slackNotificationsSender, lastResortLog)
                : null;

            var log = new LykkeLogToAzureStorage(
                applicationName,
                persistenceManager,
                slackNotificationsManager,
                lastResortLog,
                maxBatchLifetime,
                maxBatchSize,
                ownPersistenceManager: true,
                ownSlackNotificationsManager: true);

            log.Start();

            serviceCollection.AddSingleton<ILog>(log);

            return log;
        }
    }
}
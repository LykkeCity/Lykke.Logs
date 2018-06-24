using System;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Lykke.SlackNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs
{
    [Obsolete("Use new Lykke logging system and extensions from the Lykke.Logs.LoggingServiceCollectionExtensions")]
    [PublicAPI]
    public static class LykkeLogToAzureBinder
    {
        /// <param name="serviceCollection">Service collection to which log instance will be added</param>
        /// <param name="connectionString">Connection string's realoading manager</param>
        /// <param name="slackNotificationsSender">Slack notification sender to which warnings and errors will be forwarded</param>
        /// <param name="tableName">Log's table name. Default is "Logs"</param>
        /// <param name="lastResortLog">Last resort log (e.g. Console), which will be used to log logging infrastructure's issues</param>
        /// <param name="maxBatchLifetime">Log entries batch's lifetime, when exceeded, batch will be saved, and new batch will be started. Default is 5 seconds</param>
        /// <param name="batchSizeThreshold">Log messages batch's max size, when exceeded, batch will be saved, and new batch will be started. Default is 100 entries</param>
        /// <param name="disableSlackAntiSpam">Flag for slack antispam protection control</param>
        public static LykkeLogToAzureStorage UseLogToAzureStorage(this IServiceCollection serviceCollection,
            IReloadingManager<string> connectionString,
            ISlackNotificationsSender slackNotificationsSender = null,
            string tableName = "Logs",
            ILog lastResortLog = null,
            TimeSpan? maxBatchLifetime = null,
            int batchSizeThreshold = 100,
            bool disableSlackAntiSpam = false)
        {
            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                AzureTableStorage<LogEntity>.Create(connectionString, tableName, lastResortLog),
                lastResortLog);

            var slackNotificationsManager = slackNotificationsSender != null
                ? new LykkeLogToAzureSlackNotificationsManager(slackNotificationsSender, disableSlackAntiSpam, lastResortLog)
                : null;

            var log = new LykkeLogToAzureStorage(
                persistenceManager,
                slackNotificationsManager,
                lastResortLog,
                maxBatchLifetime,
                batchSizeThreshold,
                ownPersistenceManager: true,
                ownSlackNotificationsManager: true);

            log.Start();

            if (lastResortLog == null)
            {
                serviceCollection.AddSingleton<ILog>(log);
            }
            else
            {
                var aggregatedLog = new AggregateLogger();
                aggregatedLog.AddLog(lastResortLog);
                aggregatedLog.AddLog(log);
                serviceCollection.AddSingleton<ILog>(aggregatedLog);
            }
            return log;
        }
    }
}
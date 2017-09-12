using System;
using AzureStorage;
using Common.Log;

namespace Lykke.Logs
{
    public class LykkeLogToAzureStoragePersistenceManager : 
        LogPersistenceManager<LogEntity>,
        ILykkeLogToAzureStoragePersistenceManager
    {
        /// <inheritdoc />
        public LykkeLogToAzureStoragePersistenceManager(
            string componentName,
            INoSQLTableStorage<LogEntity> tableStorage,
            ILog lastResortLog = null,
            int maxRetriesCount = 10,
            TimeSpan? retryDelay = null) :

            base(
                componentName, 
                tableStorage,
                new LykkeLogToAzureEntityRowKeyGenerator(),
                lastResortLog,
                maxRetriesCount,
                retryDelay)
        {
        }
    }
}
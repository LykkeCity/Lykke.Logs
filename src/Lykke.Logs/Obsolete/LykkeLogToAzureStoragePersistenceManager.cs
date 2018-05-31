using System;
using AzureStorage;
using Common.Log;

namespace Lykke.Logs
{
    [Obsolete("Use AzureTableLogPersistenceQueue")]
    public class LykkeLogToAzureStoragePersistenceManager : 
        LogPersistenceManager<LogEntity>,
        ILykkeLogToAzureStoragePersistenceManager
    {
        /// <inheritdoc />
        public LykkeLogToAzureStoragePersistenceManager(
            string componentName,
            INoSQLTableStorage<LogEntity> tableStorage,
            ILog lastResortLog = null)
            : base(
                componentName,
                tableStorage,
                new LykkeLogToAzureEntityRowKeyGenerator(),
                lastResortLog)
        {
        }

        /// <inheritdoc />
        public LykkeLogToAzureStoragePersistenceManager(
            INoSQLTableStorage<LogEntity> tableStorage,
            ILog lastResortLog = null)
            : base(
                tableStorage,
                new LykkeLogToAzureEntityRowKeyGenerator(),
                lastResortLog)
        {
        }
    }
}
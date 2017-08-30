using AzureStorage;
using Common.Log;

namespace Lykke.Logs
{
    public class LykkeLogToAzureStoragePersistenceManager : 
        LogPersistenceManager<LogEntity>,
        ILykkeLogToAzureStoragePersistenceManager
    {
        public LykkeLogToAzureStoragePersistenceManager(
            string componentName,
            INoSQLTableStorage<LogEntity> tableStorage,
            ILog lastResortLog = null,
            int maxRetriesCount = 10) :

            base(
                componentName, 
                tableStorage,
                new LykkeLogToAzureEntityRowKeyGenerator(),
                lastResortLog,
                maxRetriesCount)
        {
        }
    }
}
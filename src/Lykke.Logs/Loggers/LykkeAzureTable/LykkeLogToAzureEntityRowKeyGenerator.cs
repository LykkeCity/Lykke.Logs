using Lykke.Logs.AzureTablePersistence;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    internal sealed class LykkeLogToAzureEntityRowKeyGenerator : ILogEntityRowKeyGenerator<LogEntity>
    {
        public string Generate(LogEntity entity, int retryNum, int batchItemNum)
        {
            return LogEntity.GenerateRowKey(entity.DateTime, batchItemNum, retryNum);
        }
    }
}
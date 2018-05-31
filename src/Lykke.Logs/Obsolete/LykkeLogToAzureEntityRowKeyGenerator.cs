using System;
using Lykke.Logs.AzureTablePersistence;

namespace Lykke.Logs
{
    [Obsolete("Use new Lykke logging system")]
    public class LykkeLogToAzureEntityRowKeyGenerator : ILogEntityRowKeyGenerator<LogEntity>
    {
        public string Generate(LogEntity entity, int retryNum, int batchItemNum)
        {
            return LogEntity.GenerateRowKey(entity.DateTime, batchItemNum, retryNum);
        }
    }
}
namespace Lykke.Logs
{
    public class LykkeLogToAzureEntityRowKeyGenerator : ILogEntityRowKeyGenerator<LogEntity>
    {
        public string Generate(LogEntity entity, int retryNum, int batchItemNum)
        {
            return LogEntity.GenerateRowKey(entity.DateTime, retryNum, batchItemNum);
        }
    }
}
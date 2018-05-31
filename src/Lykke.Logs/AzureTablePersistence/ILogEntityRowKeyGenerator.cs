using JetBrains.Annotations;

namespace Lykke.Logs.AzureTablePersistence
{
    /// <summary>
    /// Log entry row key generator abstraction. Used by <see cref="AzureTableLogPersistenceQueue{TLogEntity}"/> to 
    /// generate log entries row key 
    /// </summary>
    /// <typeparam name="TLogEntity">Log entry type</typeparam>
    [PublicAPI]
    public interface ILogEntityRowKeyGenerator<in TLogEntity>
    {
        string Generate(TLogEntity entity, int retryNum, int batchItemNum);
    }
}
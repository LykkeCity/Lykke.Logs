using System;

namespace Lykke.Logs
{
    /// <summary>
    /// Log entry row key generator abstraction. Used by <see cref="LogPersistenceManager{TLogEntity}"/> to 
    /// generate log entries row key 
    /// </summary>
    /// <typeparam name="TLogEntity">Log entry type</typeparam>
    [Obsolete("Use new Lykke logging system")]
    public interface ILogEntityRowKeyGenerator<in TLogEntity>
    {
        string Generate(TLogEntity entity, int retryNum, int batchItemNum);
    }
}
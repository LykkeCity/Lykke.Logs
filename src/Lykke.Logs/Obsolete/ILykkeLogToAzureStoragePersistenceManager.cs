using System;

namespace Lykke.Logs
{
    [Obsolete("Use new Lykke logging system")]
    public interface ILykkeLogToAzureStoragePersistenceManager : ILogPersistenceManager<LogEntity>
    {
    }
}
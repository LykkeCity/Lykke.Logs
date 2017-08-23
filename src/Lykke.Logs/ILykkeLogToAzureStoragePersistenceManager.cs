using System.Collections.Generic;

namespace Lykke.Logs
{
    public interface ILykkeLogToAzureStoragePersistenceManager
    {
        void Persist(IEnumerable<LogEntity> entries);
    }
}
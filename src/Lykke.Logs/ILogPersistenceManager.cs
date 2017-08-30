using System.Collections.Generic;

namespace Lykke.Logs
{
    public interface ILogPersistenceManager<in TLogEntity>
    {
        void Persist(IEnumerable<TLogEntity> entries);
    }
}
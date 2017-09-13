using System.Collections.Generic;

namespace Lykke.Logs
{
    public interface ILogPersistenceManager<in TLogEntity>
    {
        void Persist(IReadOnlyList<TLogEntity> entries);
    }
}
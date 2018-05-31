using System;
using System.Collections.Generic;

namespace Lykke.Logs
{
    [Obsolete("Use new Lykke logging system")]
    public interface ILogPersistenceManager<in TLogEntity>
    {
        void Persist(IEnumerable<TLogEntity> entries);
    }
}
using System;
using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs.AzureTablePersistence
{
    /// <summary>
    /// Log entries persistence queue. Persists entries in background thread by batches
    /// </summary>
    /// <typeparam name="TLogEntity">Log entry type</typeparam>
    [PublicAPI]
    public interface IAzureTableLogPersistenceQueue<in TLogEntity> : IDisposable
        where TLogEntity : ITableEntity
    {
        /// <summary>
        /// Enqueues log entry to the persistence queue
        /// </summary>
        /// <param name="entry">Log entry</param>
        void Enqueue(TLogEntity entry);
    }
}
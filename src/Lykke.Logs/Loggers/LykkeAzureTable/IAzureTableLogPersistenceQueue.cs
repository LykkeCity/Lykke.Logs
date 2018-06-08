using System;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    internal interface IAzureTableLogPersistenceQueue : IDisposable
    {
        void Enqueue(LogEntity entry);
    }
}
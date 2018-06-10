using System.Collections.Concurrent;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    [ProviderAlias("AzureTable")]
    internal sealed class AzureTableLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, AzureTableLogger> _loggers;

        private readonly IAzureTableLogPersistenceQueue _persistenceQueue;

        public AzureTableLoggerProvider(
            [NotNull] IReloadingManager<string> connectionString, 
            [NotNull] string tableName,
            [NotNull] AzureTableLoggerOptions options)
        {
            _loggers = new ConcurrentDictionary<string, AzureTableLogger>();
            
            var storage = AzureTableStorage<LogEntity>.Create(connectionString, tableName, LogFactory.LastResort);

            _persistenceQueue = new AzureTableLogPersistenceQueue(
                storage,
                "General log",
                LogFactory.LastResort,
                options.MaxBatchLifetime,
                options.BatchSizeThreshold);
        }

        public ILogger CreateLogger(string componentName)
        {
            return _loggers.GetOrAdd(componentName, CreateLoggerImplementation);
        }

        private AzureTableLogger CreateLoggerImplementation(string componentName)
        {
            return new AzureTableLogger(componentName, _persistenceQueue);
        }

        public void Dispose()
        {
            _persistenceQueue.Dispose();
        }
    }
}
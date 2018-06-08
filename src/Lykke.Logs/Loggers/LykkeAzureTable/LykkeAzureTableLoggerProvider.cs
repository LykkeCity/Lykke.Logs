using System.Collections.Concurrent;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    [ProviderAlias("AzureTable")]
    internal sealed class LykkeAzureTableLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, LykkeAzureTableLogger> _loggers;

        private readonly IAzureTableLogPersistenceQueue _persistenceQueue;

        public LykkeAzureTableLoggerProvider(
            [NotNull] IReloadingManager<string> connectionString, 
            [NotNull] string tableName,
            [NotNull] AzureTableLoggerOptions options)
        {
            _loggers = new ConcurrentDictionary<string, LykkeAzureTableLogger>();
            
            var storage = AzureTableStorage<LogEntity>.Create(connectionString, tableName, DirectConsoleLogFactory.Instance);

            _persistenceQueue = new AzureTableLogPersistenceQueue(
                storage,
                "General log",
                DirectConsoleLogFactory.Instance,
                options.MaxBatchLifetime,
                options.BatchSizeThreshold);
        }

        public ILogger CreateLogger(string componentName)
        {
            return _loggers.GetOrAdd(componentName, CreateLoggerImplementation);
        }

        private LykkeAzureTableLogger CreateLoggerImplementation(string componentName)
        {
            return new LykkeAzureTableLogger(componentName, _persistenceQueue);
        }

        public void Dispose()
        {
            _persistenceQueue.Dispose();
        }
    }
}
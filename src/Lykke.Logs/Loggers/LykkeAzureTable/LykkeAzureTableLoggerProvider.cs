using System;
using System.Collections.Concurrent;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Logs.AzureTablePersistence;
using Lykke.SettingsReader;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    [ProviderAlias("AzureTable")]
    internal sealed class LykkeAzureTableLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, LykkeAzureTableLogger> _loggers;

        private readonly IAzureTableLogPersistenceQueue<LogEntity> _persistenceQueue;

        public LykkeAzureTableLoggerProvider(
            [NotNull] IReloadingManager<string> connectionString, 
            [NotNull] string tableName,
            [CanBeNull] TimeSpan? maxBatchLifetime = null,
            int batchSizeThreshold = 100)
        {
            _loggers = new ConcurrentDictionary<string, LykkeAzureTableLogger>();
            
            var storage = AzureTableStorage<LogEntity>.Create(connectionString, tableName, DirectConsoleLogFactory.Instance);

            _persistenceQueue = new AzureTableLogPersistenceQueue<LogEntity>(
                storage,
                new LykkeLogToAzureEntityRowKeyGenerator(),
                "General log",
                DirectConsoleLogFactory.Instance,
                maxBatchLifetime,
                batchSizeThreshold,
                // 2 is enough since we have yyyy-MM-dd as PK and batch will not be greater than a day for sure.
                degreeOfPersistenceParallelism: 2);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        private LykkeAzureTableLogger CreateLoggerImplementation(string categoryName)
        {
            return new LykkeAzureTableLogger(categoryName, _persistenceQueue);
        }

        public void Dispose()
        {
            _persistenceQueue.Dispose();
        }
    }
}
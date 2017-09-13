using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables.Decorators;
using Common;
using Common.Log;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs
{
    public class LogPersistenceManager<TLogEntity> :
        ProducerConsumer<IReadOnlyList<TLogEntity>>,
        ILogPersistenceManager<TLogEntity> 
        
        where TLogEntity : ITableEntity, new()
    {
        private readonly INoSQLTableStorage<TLogEntity> _tableStorage;
        private readonly ILogEntityRowKeyGenerator<TLogEntity> _rowKeyGenerator;

        /// <param name="componentName"></param>
        /// <param name="tableStorage"></param>
        /// <param name="rowKeyGenerator"></param>
        /// <param name="lastResortLog"></param>
        /// <param name="maxRetriesCount">Max count of retries on insert failure</param>
        /// <param name="retryDelay">Gap between retries on insert failure. Default value is 5 seconds</param>
        public LogPersistenceManager(
            string componentName,
            INoSQLTableStorage<TLogEntity> tableStorage,
            ILogEntityRowKeyGenerator<TLogEntity> rowKeyGenerator,
            ILog lastResortLog = null,
            int maxRetriesCount = 10,
            TimeSpan? retryDelay = null) :

            base(componentName, lastResortLog)
        {
            _tableStorage = new RetryOnFailureAzureTableStorageDecorator<TLogEntity>(
                tableStorage,
                maxRetriesCount,
                retryDelay: retryDelay ?? TimeSpan.FromSeconds(5));

            _rowKeyGenerator = rowKeyGenerator;
        }

        public void Persist(IReadOnlyList<TLogEntity> entries)
        {
            Produce(entries);
        }

        protected override async Task Consume(IReadOnlyList<TLogEntity> entries)
        {
            await _tableStorage.InsertBatchAndGenerateRowKeyAsync(
                    entries,
                    (entity, retryNum, batchItemNum) => _rowKeyGenerator.Generate(entity, retryNum, batchItemNum));
        }
    }
}
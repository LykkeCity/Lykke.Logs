using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Log;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs
{
    public class LogPersistenceManager<TLogEntity> : ProducerConsumer<IEnumerable<TLogEntity>>, ILogPersistenceManager<TLogEntity> 
        where TLogEntity : ITableEntity, new()
    {
        private readonly INoSQLTableStorage<TLogEntity> _tableStorage;
        private readonly ILogEntityRowKeyGenerator<TLogEntity> _rowKeyGenerator;
        private readonly ILog _lastResortLog;

        /// <param name="componentName"></param>
        /// <param name="tableStorage"></param>
        /// <param name="rowKeyGenerator"></param>
        /// <param name="lastResortLog"></param>
        public LogPersistenceManager(
            string componentName,
            INoSQLTableStorage<TLogEntity> tableStorage,
            ILogEntityRowKeyGenerator<TLogEntity> rowKeyGenerator,
            ILog lastResortLog = null)
            : base(componentName, lastResortLog)
        {
            _tableStorage = tableStorage;
            _rowKeyGenerator = rowKeyGenerator;
            _lastResortLog = lastResortLog ?? new LogToConsole();
        }

        /// <param name="tableStorage"></param>
        /// <param name="rowKeyGenerator"></param>
        /// <param name="lastResortLog"></param>
        public LogPersistenceManager(
            INoSQLTableStorage<TLogEntity> tableStorage,
            ILogEntityRowKeyGenerator<TLogEntity> rowKeyGenerator,
            ILog lastResortLog = null)
            : base(lastResortLog)
        {
            _tableStorage = tableStorage;
            _rowKeyGenerator = rowKeyGenerator;
            _lastResortLog = lastResortLog ?? new LogToConsole();
        }

        public void Persist(IEnumerable<TLogEntity> entries)
        {
            Produce(entries);
        }

        protected override async Task Consume(IEnumerable<TLogEntity> entries)
        {
            try
            {
                var partitionGroups = entries.GroupBy(e => e.PartitionKey);
                var tasks = partitionGroups
                    .Select(group => _tableStorage.InsertBatchAndGenerateRowKeyAsync(
                        group.ToArray(),
                        (entity, retryNum, batchItemNum) => _rowKeyGenerator.Generate(entity, retryNum, batchItemNum)));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                await _lastResortLog.WriteErrorAsync(
                    "Persist log entries to the Table Storage",
                    $"Manager type: {GetType().Name}. Log entity type: {typeof(TLogEntity).Name}",
                    ex);
            }
        }
    }
}
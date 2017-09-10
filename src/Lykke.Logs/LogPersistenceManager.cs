using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Log;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs
{
    public class LogPersistenceManager<TLogEntity> :
        ProducerConsumer<IEnumerable<TLogEntity>>,
        ILogPersistenceManager<TLogEntity> 
        
        where TLogEntity : ITableEntity, new()
    {
        private readonly INoSQLTableStorage<TLogEntity> _tableStorage;
        private readonly ILogEntityRowKeyGenerator<TLogEntity> _rowKeyGenerator;
        private readonly int _maxRetriesCount;

        public LogPersistenceManager(
            string componentName,
            INoSQLTableStorage<TLogEntity> tableStorage,
            ILogEntityRowKeyGenerator<TLogEntity> rowKeyGenerator,
            ILog lastResortLog = null,
            int maxRetriesCount = 10) :
            base(componentName, lastResortLog)
        {
            _tableStorage = tableStorage;
            _rowKeyGenerator = rowKeyGenerator;
            _maxRetriesCount = maxRetriesCount;
        }

        public void Persist(IEnumerable<TLogEntity> entries)
        {
            Produce(entries);
        }

        protected override async Task Consume(IEnumerable<TLogEntity> entries)
        {
            var partitionGroups = entries.GroupBy(e => e.PartitionKey);
            var tasks = partitionGroups.Select(group => SavePartitionGroupAsync(group.ToArray()));

            await Task.WhenAll(tasks);
        }

        private async Task SavePartitionGroupAsync(IReadOnlyList<TLogEntity> group)
        {
            var retryNumber = 0;

            UpdateRowKeys(group, retryNumber);

            while (true)
            {
                ++retryNumber;

                try
                {
                    await _tableStorage.InsertAsync(group);
                    return;
                }
                catch (AggregateException ex)
                    when ((ex.InnerExceptions[0] as StorageException)?.RequestInformation?.HttpStatusCode ==
                          (int)HttpStatusCode.Conflict && retryNumber <= _maxRetriesCount)
                {
                    UpdateRowKeys(group, retryNumber);
                }
                catch (StorageException ex)
                    when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict && retryNumber <= _maxRetriesCount)
                {
                    UpdateRowKeys(group, retryNumber);
                }
                catch (AggregateException ex)
                    when ((ex.InnerExceptions[0] as StorageException)?.RequestInformation?.HttpStatusCode !=
                          (int)HttpStatusCode.BadRequest && retryNumber <= _maxRetriesCount)
                {
                }
                catch (StorageException ex)
                    when (ex.RequestInformation.HttpStatusCode != (int)HttpStatusCode.BadRequest && retryNumber <= _maxRetriesCount)
                {
                }
            }
        }

        private void UpdateRowKeys(IReadOnlyList<TLogEntity> group, int retryNumber)
        {
            for (var itemNumber = 0; itemNumber < group.Count; ++itemNumber)
            {
                var entry = group[itemNumber];

                entry.RowKey = _rowKeyGenerator.Generate(entry, retryNumber, itemNumber);
            }
        }
    }
}
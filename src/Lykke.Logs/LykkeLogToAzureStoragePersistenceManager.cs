using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Log;
using Microsoft.WindowsAzure.Storage;

namespace Lykke.Logs
{
    public class LykkeLogToAzureStoragePersistenceManager : 
        ProducerConsumer<IEnumerable<LogEntity>>,
        ILykkeLogToAzureStoragePersistenceManager
    {
        private readonly INoSQLTableStorage<LogEntity> _tableStorage;
        private readonly int _maxRetriesCount;

        public LykkeLogToAzureStoragePersistenceManager(
            string componentName,
            INoSQLTableStorage<LogEntity> tableStorage,
            ILog lastResortLog = null,
            int maxRetriesCount = 10) :
            base(componentName, lastResortLog)
        {
            _tableStorage = tableStorage;
            _maxRetriesCount = maxRetriesCount;
        }

        public void Persist(IEnumerable<LogEntity> entries)
        {
            Produce(entries);
        }

        protected override async Task Consume(IEnumerable<LogEntity> entries)
        {
            var partitionGroups = entries.GroupBy(e => e.PartitionKey);
            var tasks = partitionGroups.Select(group => SavePartitionGroupAsync(group.ToArray()));
            
            await Task.WhenAll(tasks);
        }

        private async Task SavePartitionGroupAsync(IReadOnlyList<LogEntity> group)
        {
            var retryNumber = 0;
            
            while (true)
            {
                try
                {
                    await _tableStorage.InsertAsync(group);
                    return;
                }
                catch (AggregateException ex)
                    when ((ex.InnerExceptions[0] as StorageException)?.RequestInformation?.HttpStatusCode ==
                          (int) HttpStatusCode.Conflict && retryNumber < _maxRetriesCount)
                {
                    IncrementRowKeys(group, retryNumber);
                }
                catch (StorageException ex)
                    when (ex.RequestInformation.HttpStatusCode == (int) HttpStatusCode.Conflict && retryNumber < _maxRetriesCount)
                {
                    IncrementRowKeys(group, retryNumber);
                }
                catch (AggregateException ex)
                    when ((ex.InnerExceptions[0] as StorageException)?.RequestInformation?.HttpStatusCode !=
                          (int) HttpStatusCode.BadRequest && retryNumber < _maxRetriesCount)
                {
                }
                catch (StorageException ex)
                    when (ex.RequestInformation.HttpStatusCode != (int) HttpStatusCode.BadRequest && retryNumber < _maxRetriesCount)
                {
                }

                ++retryNumber;
            }
        }

        private static void IncrementRowKeys(IReadOnlyList<LogEntity> group, int retryNumber)
        {
            for (var itemNumber = 0; itemNumber < @group.Count; ++itemNumber)
            {
                var entry = group[itemNumber];

                entry.RowKey = LogEntity.GenerateRowKey(entry.DateTime, retryNumber, itemNumber);
            }
        }
    }
}
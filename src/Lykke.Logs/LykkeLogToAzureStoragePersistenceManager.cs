using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Log;
using Microsoft.WindowsAzure.Storage;

namespace Lykke.Logs
{
    public class LykkeLogToAzureStoragePersistenceManager : 
        ProducerConsumer<IEnumerable<LogEntity>>,
        ILykkeLogToAzureStoragePersistenceManager,
        IDisposable
    {
        private readonly INoSQLTableStorage<LogEntity> _tableStorage;

        public LykkeLogToAzureStoragePersistenceManager(
            string componentName,
            INoSQLTableStorage<LogEntity> tableStorage,
            ILog lastResortLog = null) :
            base(componentName, lastResortLog)
        {
            _tableStorage = tableStorage;
        }

        public void Persist(IEnumerable<LogEntity> entries)
        {
            Produce(entries);
        }

        public void Dispose()
        {
            Stop();
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
                    when ((ex.InnerExceptions[0] as StorageException)?.RequestInformation?.HttpStatusCode == 409)
                {
                }
                catch (StorageException ex)
                    when (ex.RequestInformation.HttpStatusCode == 409)
                {
                }

                ++retryNumber;

                for (var itemNumber = 0; itemNumber < group.Count; ++itemNumber)
                {
                    var entry = group[itemNumber];

                    entry.RowKey = LogEntity.GenerateRowKey(entry.DateTime, retryNumber, itemNumber);
                }

                if (retryNumber > 999)
                {
                    throw new InvalidOperationException("Couldn't save entries to log");
                }
            }
        }
    }
}
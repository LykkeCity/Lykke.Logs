using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using AzureStorage;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    internal sealed class AzureTableLogPersistenceQueue : IAzureTableLogPersistenceQueue
    {
        [NotNull] private readonly ITimerTrigger _timer;
        [NotNull] private readonly INoSQLTableStorage<LogEntity> _storage;

        private readonly TimeSpan _maxBatchLifetime;

        [NotNull] private readonly BatchBlock<LogEntity> _batchBlock;
        [NotNull] private readonly TransformManyBlock<LogEntity[], IGrouping<string, LogEntity>> _groupBatchBlock;
        [NotNull] private readonly ActionBlock<IGrouping<string, LogEntity>> _persistGroupBlock;

        private DateTime _currentBatchExpirationMoment;

        public AzureTableLogPersistenceQueue(
            [NotNull] INoSQLTableStorage<LogEntity> storage,
            [NotNull] string logName,
            [NotNull] ILogFactory lastResortLogFactory,
            TimeSpan maxBatchLifetime,
            int batchSizeThreshold)
        {
            if (string.IsNullOrEmpty(logName))
            {
                throw new ArgumentException("Should be not empty string", nameof(logName));
            }
            if (maxBatchLifetime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBatchLifetime), maxBatchLifetime, "Should be positive time span");
            }
            if (batchSizeThreshold < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSizeThreshold), batchSizeThreshold, "Should be positive number");
            }

            _maxBatchLifetime = maxBatchLifetime;

            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            _batchBlock = new BatchBlock<LogEntity>(batchSizeThreshold);
            _groupBatchBlock = new TransformManyBlock<LogEntity[], IGrouping<string, LogEntity>>(batch => GroupEntriesBatch(batch));
            _persistGroupBlock = new ActionBlock<IGrouping<string, LogEntity>>(
                // ReSharper disable once ConvertClosureToMethodGroup
                group => PersistEntriesGroup(group),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 2
                });

            _batchBlock.LinkTo(_groupBatchBlock);
            _groupBatchBlock.LinkTo(_persistGroupBlock);

            ExtendBatchExpiration();

            _timer = new TimerTrigger(logName, TimeSpan.FromMilliseconds(50), lastResortLogFactory)
                .DisableTelemetry();
            _timer.Triggered += HandleTimerTriggered;
            _timer.Start();
        }

        /// <inheritdoc />
        public void Enqueue(LogEntity entry)
        {
            _batchBlock.Post(entry);
        }

        public void Dispose()
        {
            _batchBlock.Complete();
            _batchBlock.Completion.ConfigureAwait(false).GetAwaiter().GetResult();
            
            _groupBatchBlock.Complete();
            _groupBatchBlock.Completion.ConfigureAwait(false).GetAwaiter().GetResult();

            _persistGroupBlock.Complete();
            _persistGroupBlock.Completion.ConfigureAwait(false).GetAwaiter().GetResult();

            _timer.Dispose();
        }

        private IEnumerable<IGrouping<string, LogEntity>> GroupEntriesBatch(LogEntity[] batch)
        {
            ExtendBatchExpiration();
            
            return batch.GroupBy(e => e.PartitionKey);
        }

        private Task PersistEntriesGroup(IGrouping<string, LogEntity> group)
        {
            return _storage.InsertBatchAndGenerateRowKeyAsync(
                group.ToArray(),
                (entity, retryNum, batchItemNum) => LogEntity.GenerateRowKey(entity.DateTime, batchItemNum, retryNum));
        }

        private Task HandleTimerTriggered(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
        {
            if (DateTime.UtcNow > _currentBatchExpirationMoment)
            {
                _batchBlock.TriggerBatch();

                ExtendBatchExpiration();
            }

            return Task.CompletedTask;
        }

        private void ExtendBatchExpiration()
        {
            _currentBatchExpirationMoment = DateTime.UtcNow + _maxBatchLifetime;
        }
    }
}
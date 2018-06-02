using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using AzureStorage;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs.AzureTablePersistence
{
    /// <summary>
    /// Log entries persistence queue. Persists entries in background thread by batches
    /// </summary>
    /// <typeparam name="TLogEntity">Log entry type</typeparam>
    [PublicAPI]
    public sealed class AzureTableLogPersistenceQueue<TLogEntity> : IAzureTableLogPersistenceQueue<TLogEntity>
        where TLogEntity : ITableEntity, new()
    {
        [NotNull] private readonly ITimerTrigger _timer;
        [NotNull] private readonly INoSQLTableStorage<TLogEntity> _storage;
        [NotNull] private readonly ILog _log;
        [NotNull] private readonly ILogEntityRowKeyGenerator<TLogEntity> _rowKeyGenerator;

        private readonly TimeSpan _maxBatchLifetime;
        private readonly int _batchSizeThreshold;
        private readonly int _degreeOfPersistenceParallelism;

        [NotNull] private readonly BatchBlock<TLogEntity> _batchBlock;
        [NotNull] private readonly TransformManyBlock<TLogEntity[], IGrouping<string, TLogEntity>> _groupBatchBlock;
        [NotNull] private readonly ActionBlock<IGrouping<string, TLogEntity>> _persistGroupBlock;

        private DateTime _currentBatchExpirationMoment;

        /// <summary>
        /// Creates <see cref="AzureTableLogPersistenceQueue{TLogEntity}"/>
        /// </summary>
        /// <param name="storage">Storage to which log entries should be persisted</param>
        /// <param name="rowKeyGenerator">Log entries row keys generator</param>
        /// <param name="logName">Name of the log. Will be used to log failures to the <paramref name="lastResortLogFactory"/> log</param>
        /// <param name="lastResortLogFactory">Last resort log factory. Usually <see cref="LastResortLogFactory.Instance"/></param>
        /// <param name="maxBatchLifetime">
        /// Max time for which entries will be keeped in the in-memory buffer before they will be persisted.
        /// This setting affects max latency before entry will be persisted.
        /// Default value 5 seconds
        /// </param>
        /// <param name="batchSizeThreshold">
        /// Amount of entries that triggers batch persisting
        /// </param>
        /// <param name="degreeOfPersistenceParallelism">
        /// Max parallel threads, which could be used to persist log entries groups.
        /// Each group contains only signle partiotion key. So, if your log entriy have partition key per day,
        /// then <paramref name="degreeOfPersistenceParallelism"/> of 2 is enough for you.
        /// </param>
        public AzureTableLogPersistenceQueue(
            [NotNull] INoSQLTableStorage<TLogEntity> storage,
            [NotNull] ILogEntityRowKeyGenerator<TLogEntity> rowKeyGenerator,
            [NotNull] string logName,
            [CanBeNull] ILogFactory lastResortLogFactory = null,
            [CanBeNull] TimeSpan? maxBatchLifetime = null,
            int batchSizeThreshold = 100,
            int degreeOfPersistenceParallelism = 10)
        {
            if (string.IsNullOrEmpty(logName))
            {
                throw new ArgumentException("Should be not empty string", nameof(logName));
            }
            if (batchSizeThreshold < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSizeThreshold), batchSizeThreshold, "Should be positive number");
            }
            if (degreeOfPersistenceParallelism < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(degreeOfPersistenceParallelism), degreeOfPersistenceParallelism, "Should be positive number");
            }

            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _rowKeyGenerator = rowKeyGenerator ?? throw new ArgumentNullException(nameof(rowKeyGenerator));
            _batchSizeThreshold = batchSizeThreshold;
            _degreeOfPersistenceParallelism = degreeOfPersistenceParallelism;
            _maxBatchLifetime = maxBatchLifetime ?? TimeSpan.FromSeconds(5);

            lastResortLogFactory = lastResortLogFactory ?? LastResortLogFactory.Instance;

            _log = lastResortLogFactory.CreateLog(this);

            _batchBlock = new BatchBlock<TLogEntity>(batchSizeThreshold);
            _groupBatchBlock = new TransformManyBlock<TLogEntity[], IGrouping<string, TLogEntity>>(batch => GroupEntriesBatch(batch));
            _persistGroupBlock = new ActionBlock<IGrouping<string, TLogEntity>>(
                // ReSharper disable once ConvertClosureToMethodGroup
                group => PersistEntriesGroup(group),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = degreeOfPersistenceParallelism
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
        public void Enqueue(TLogEntity entry)
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

        private IEnumerable<IGrouping<string, TLogEntity>> GroupEntriesBatch(TLogEntity[] batch)
        {
            ExtendBatchExpiration();
            
            return batch.GroupBy(e => e.PartitionKey);
        }

        private Task PersistEntriesGroup(IGrouping<string, TLogEntity> group)
        {
            return _storage.InsertBatchAndGenerateRowKeyAsync(
                group.ToArray(),
                (entity, retryNum, batchItemNum) => _rowKeyGenerator.Generate(entity, retryNum, batchItemNum));
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
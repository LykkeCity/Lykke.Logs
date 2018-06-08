using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Logs.Loggers.LykkeAzureTable;
using NSubstitute;
using Xunit;

// ReSharper disable RedundantArgumentDefaultValue

namespace Lykke.Logs.Tests
{
    public class AzureTablePersistenceQueueTests
    {
        [Fact]
        public async Task Batch_persistence_is_not_triggered_if_there_are_no_entries()
        {
            var storage = Substitute.For<INoSQLTableStorage<Loggers.LykkeAzureTable.LogEntity>>();

            using (new AzureTableLogPersistenceQueue(
                storage,
                "test log",
                maxBatchLifetime: TimeSpan.FromMilliseconds(100),
                batchSizeThreshold: 1,
                lastResortLogFactory: DirectConsoleLogFactory.Instance))
            {
                // Lets batch life time to be elxpired

                await Task.Delay(TimeSpan.FromMilliseconds(200));

                await storage
                    .DidNotReceive()
                    .InsertAsync(Arg.Any<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>());
            }
        }

        [Fact]
        public async Task Batch_persistence_is_not_triggered_if_no_entries_amount_nor_batch_lifetime_are_expired()
        {
            var storage = Substitute.For<INoSQLTableStorage<Loggers.LykkeAzureTable.LogEntity>>();

            using (var queue = new AzureTableLogPersistenceQueue(
                storage,
                "test log",
                maxBatchLifetime: TimeSpan.FromHours(1),
                batchSizeThreshold: 100,
                lastResortLogFactory: DirectConsoleLogFactory.Instance))
            {
                for (var i = 0; i < 90; ++i)
                {
                    queue.Enqueue(new Loggers.LykkeAzureTable.LogEntity());
                }

                // Lets batch, if any, to be performed

                await Task.Delay(TimeSpan.FromMilliseconds(50));

                await storage
                    .DidNotReceive()
                    .InsertAsync(Arg.Any<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>());
            }
        }

        [Fact]
        public async Task Batch_persistence_is_triggered_by_entries_amount()
        {
            var storage = Substitute.For<INoSQLTableStorage<Loggers.LykkeAzureTable.LogEntity>>();

            using (var queue = new AzureTableLogPersistenceQueue(
                storage,
                "test log",
                maxBatchLifetime: TimeSpan.FromHours(1),
                batchSizeThreshold: 100,
                lastResortLogFactory: DirectConsoleLogFactory.Instance))
            {
                for (var i = 0; i < 100; ++i)
                {
                    queue.Enqueue(new Loggers.LykkeAzureTable.LogEntity());
                }

                // Lets batch to be performed

                await Task.Delay(TimeSpan.FromMilliseconds(50));

                await storage
                    .Received(1)
                    .InsertAsync(Arg.Is<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>(entities => entities.Count() == 100));
            }
        }

        [Fact]
        public async Task Batch_persistence_is_triggered_by_time_expiration()
        {
            var storage = Substitute.For<INoSQLTableStorage<Loggers.LykkeAzureTable.LogEntity>>();

            storage.InsertAsync(Arg.Any<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>()).Returns(callInfo =>
            {
                var logEntities = callInfo.Arg<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>();

                foreach (var entity in logEntities)
                {
                    Assert.InRange(DateTimeOffset.UtcNow - entity.DateTime, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(1300));
                }
                
                return Task.CompletedTask;
            });

            using (var queue = new AzureTableLogPersistenceQueue(
                storage,
                "test log",
                maxBatchLifetime: TimeSpan.FromSeconds(1),
                batchSizeThreshold: 100,
                lastResortLogFactory: DirectConsoleLogFactory.Instance))
            {
                queue.Enqueue(new Loggers.LykkeAzureTable.LogEntity
                {
                    DateTime = DateTime.UtcNow
                });

                // Lets batch lifetime to be ellapse

                await Task.Delay(TimeSpan.FromSeconds(2));

                await storage
                    .Received(1)
                    .InsertAsync(Arg.Is<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>(entities => entities.Count() == 1));
            }
        }

        [Fact]
        public async Task Batch_persistence_is_triggered_by_entries_amount_extends_batch_lifetime()
        {
            var storage = Substitute.For<INoSQLTableStorage<Loggers.LykkeAzureTable.LogEntity>>();
            var batchTimes = new List<DateTime>();
            var batchCounts = new List<int>();

            storage.InsertAsync(Arg.Any<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>()).Returns(callInfo =>
            {
                batchTimes.Add(DateTime.UtcNow);
                batchCounts.Add(callInfo.Arg<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>().Count());

                return Task.CompletedTask;
            });

            using (var queue = new AzureTableLogPersistenceQueue(
                storage,
                "test log",
                maxBatchLifetime: TimeSpan.FromSeconds(1),
                batchSizeThreshold: 10,
                lastResortLogFactory: DirectConsoleLogFactory.Instance))
            {
                for (var i = 0; i < 9; ++i)
                {
                    queue.Enqueue(new Loggers.LykkeAzureTable.LogEntity());
                }

                // Wait a half of batch life time

                await Task.Delay(TimeSpan.FromMilliseconds(500));

                // Completes the batch

                queue.Enqueue(new Loggers.LykkeAzureTable.LogEntity());

                // Lets batch to be performed

                await Task.Delay(TimeSpan.FromMilliseconds(50));

                // Fills new batch

                queue.Enqueue(new Loggers.LykkeAzureTable.LogEntity());

                // Lets batch lifetime to be ellapsed

                await Task.Delay(TimeSpan.FromSeconds(2));

                Assert.Equal(2, batchTimes.Count);
                Assert.InRange(batchTimes.Last() - batchTimes.First(), TimeSpan.FromMilliseconds(850), TimeSpan.FromMilliseconds(1150));
                Assert.Equal(10, batchCounts.First());
                Assert.Equal(1, batchCounts.Last());
            }
        }

        [Fact]
        public async Task Batch_persistence_is_triggered_by_disposing()
        {
            var storage = Substitute.For<INoSQLTableStorage<Loggers.LykkeAzureTable.LogEntity>>();

            using (var queue = new AzureTableLogPersistenceQueue(
                storage,
                "test log",
                maxBatchLifetime: TimeSpan.FromHours(1),
                batchSizeThreshold: 10,
                lastResortLogFactory: DirectConsoleLogFactory.Instance))
            {
                queue.Enqueue(new Loggers.LykkeAzureTable.LogEntity());
            }

            await storage
                .Received(1)
                .InsertAsync(Arg.Is<IEnumerable<Loggers.LykkeAzureTable.LogEntity>>(entities => entities.Count() == 1));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Lykke.Logs.Tests
{
    public class LykkeLogToAzureStorageTests
    {
        private readonly ILykkeLogToAzureStoragePersistenceManager _persistenceManagerMock;
        private readonly ILykkeLogToAzureSlackNotificationsManager _slackNotificationsManagerMock;

        public LykkeLogToAzureStorageTests()
        {
            _persistenceManagerMock = Substitute.For<ILykkeLogToAzureStoragePersistenceManager>();
            _slackNotificationsManagerMock = Substitute.For<ILykkeLogToAzureSlackNotificationsManager>();
        }

        [Fact(Skip = "Test is hunds up on TC. Reason is unknown, it seems that TimePeriod is not executed")]
        public void Test_that_batch_is_not_saved_when_lifetime_and_size_is_not_exceeded()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", _persistenceManagerMock, maxBatchLifetime: TimeSpan.FromSeconds(100), batchSizeThreshold: 100);

            log.Start();

            // Act
            for (var i = 0; i < 15; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            // Assert
            _persistenceManagerMock.DidNotReceiveWithAnyArgs().Persist(null);

            log.Dispose();
        }

        [Fact(Skip = "Test is hunds up on TC. Reason is unknown, it seems that TimePeriod is not executed")]
        public void Test_that_batch_is_saved_when_size_exceeded()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", _persistenceManagerMock, maxBatchLifetime: TimeSpan.FromSeconds(100), batchSizeThreshold: 10);

            log.Start();

            // Act
            for (var i = 0; i < 15; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();

            // Assert
            _persistenceManagerMock.Received().Persist(Arg.Is<IReadOnlyCollection<LogEntity>>(e => e.Count >= 10));
            _persistenceManagerMock.DidNotReceive().Persist(Arg.Is<IReadOnlyCollection<LogEntity>>(e => e.Count < 10));

            log.Dispose();
        }

        [Fact(Skip = "Test is hunds up on TC. Reason is unknown, it seems that TimePeriod is not executed")]
        public void Test_that_batch_is_saved_when_lifetime_is_exceeded()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", _persistenceManagerMock, maxBatchLifetime: TimeSpan.FromSeconds(1), batchSizeThreshold: 100);

            log.Start();

            // Act
            for (var i = 0; i < 15; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromSeconds(1.5)).Wait();

            // Assert
            _persistenceManagerMock.Received().Persist(Arg.Is<IReadOnlyCollection<LogEntity>>(e => e.Count == 15));
            _persistenceManagerMock.DidNotReceive().Persist(Arg.Is<IReadOnlyCollection<LogEntity>>(e => e.Count != 15));

            log.Dispose();
        }

        [Fact(Skip = "Test is hunds up on TC. Reason is unknown, it seems that TimePeriod is not executed")]
        public void Test_that_batch_is_saved_when_lifetime_is_exceeded_and_then_when_size_is_exceeded()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", _persistenceManagerMock, maxBatchLifetime: TimeSpan.FromSeconds(1), batchSizeThreshold: 10);

            log.Start();

            // Act
            for (var i = 0; i < 5; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromSeconds(1.5)).Wait();

            for (var i = 0; i < 18; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();

            // Assert
            _persistenceManagerMock.Received().Persist(Arg.Is<IReadOnlyCollection<LogEntity>>(e => e.Count == 5));
            _persistenceManagerMock.Received().Persist(Arg.Is<IReadOnlyCollection<LogEntity>>(e => e.Count >= 10));
            _persistenceManagerMock.DidNotReceive().Persist(Arg.Is<IReadOnlyCollection<LogEntity>>(e => e.Count != 5 && e.Count < 10));

            log.Dispose();
        }

        [Fact(Skip = "Test is hunds up on TC. Reason is unknown, it seems that TimePeriod is not executed")]
        public void Test_that_slack_notifications_is_sent_despite_of_batch_size_and_lifetime()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests",
                _persistenceManagerMock, _slackNotificationsManagerMock,
                maxBatchLifetime: TimeSpan.FromSeconds(100), batchSizeThreshold: 100);

            log.Start();

            // Act
            for (var i = 0; i < 15; ++i)
            {
                log.WriteMonitorAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromSeconds(1.5)).Wait();

            // Assert
            _slackNotificationsManagerMock.Received(15).SendNotification(Arg.Any<LogEntity>());

            log.Dispose();
        }
    }
}
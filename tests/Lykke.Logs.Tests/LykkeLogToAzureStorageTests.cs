using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Lykke.Logs.Tests
{
    public class LykkeLogToAzureStorageTests
    {
        private readonly Mock<ILykkeLogToAzureStoragePersistenceManager> _persistenceManagerMock;
        private readonly Mock<ILykkeLogToAzureSlackNotificationsManager> _slackNotificationsManagerMock;

        public LykkeLogToAzureStorageTests()
        {
            _persistenceManagerMock = new Mock<ILykkeLogToAzureStoragePersistenceManager>();
            _slackNotificationsManagerMock = new Mock<ILykkeLogToAzureSlackNotificationsManager>();
        }

        [Fact]
        public void Test_that_batch_is_not_saved_when_lifetime_and_size_is_not_exceeded()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", _persistenceManagerMock.Object, maxBatchLifetime: TimeSpan.FromSeconds(100), batchSizeThreshold: 100);

            log.Start();

            // Act
            for (var i = 0; i < 15; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            // Assert
            _persistenceManagerMock.Verify(m => m.Persist(It.IsAny<IReadOnlyCollection<LogEntity>>()), Times.Never);

            log.Dispose();
        }

        [Fact]
        public void Test_that_batch_is_saved_when_size_exceeded()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", _persistenceManagerMock.Object, maxBatchLifetime: TimeSpan.FromSeconds(100), batchSizeThreshold: 10);

            log.Start();

            // Act
            for (var i = 0; i < 15; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();

            // Assert
            _persistenceManagerMock.Verify(m => m.Persist(It.Is<IReadOnlyCollection<LogEntity>>(e => e.Count >= 10)), Times.Once);
            _persistenceManagerMock.Verify(m => m.Persist(It.Is<IReadOnlyCollection<LogEntity>>(e => e.Count < 10)), Times.Never);

            log.Dispose();
        }

        [Fact]
        public void Test_that_batch_is_saved_when_lifetime_is_exceeded()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", _persistenceManagerMock.Object, maxBatchLifetime: TimeSpan.FromSeconds(1), batchSizeThreshold: 100);

            log.Start();

            // Act
            for (var i = 0; i < 15; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromSeconds(1.5)).Wait();

            // Assert
            _persistenceManagerMock.Verify(m => m.Persist(It.Is<IReadOnlyCollection<LogEntity>>(e => e.Count == 15)), Times.Once);
            _persistenceManagerMock.Verify(m => m.Persist(It.Is<IReadOnlyCollection<LogEntity>>(e => e.Count != 15)), Times.Never);

            log.Dispose();
        }

        [Fact]
        public void Test_that_batch_is_saved_when_lifetime_is_exceeded_and_then_when_size_is_exceeded()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", _persistenceManagerMock.Object, maxBatchLifetime: TimeSpan.FromSeconds(1), batchSizeThreshold: 10);

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
            _persistenceManagerMock.Verify(m => m.Persist(It.Is<IReadOnlyCollection<LogEntity>>(e => e.Count == 5)), Times.Once);
            _persistenceManagerMock.Verify(m => m.Persist(It.Is<IReadOnlyCollection<LogEntity>>(e => e.Count >= 10)), Times.Once);
            _persistenceManagerMock.Verify(m => m.Persist(It.Is<IReadOnlyCollection<LogEntity>>(e => e.Count != 5 && e.Count < 10)), Times.Never);

            log.Dispose();
        }

        [Fact]
        public void Test_that_slack_notifications_is_sent_despite_of_batch_size_and_lifetime()
        {
            // Arrange
            var log = new LykkeLogToAzureStorage("Tests", 
                _persistenceManagerMock.Object, _slackNotificationsManagerMock.Object,
                maxBatchLifetime: TimeSpan.FromSeconds(100), batchSizeThreshold: 100);

            log.Start();

            // Act
            for (var i = 0; i < 15; ++i)
            {
                log.WriteInfoAsync("Test", "", "", "");
            }

            Task.Delay(TimeSpan.FromSeconds(1.5)).Wait();

            // Assert
            _slackNotificationsManagerMock.Verify(m => m.SendNotification(It.IsAny<LogEntity>()), Times.Exactly(15));

            log.Dispose();
        }
    }
}
using System;
using System.Threading;
using Moq;
using Xunit;
using Lykke.Logs.Slack;
using Lykke.SlackNotifications;

namespace Lykke.Logs.Tests
{
    public class SlackAntispamTests
    {
        private readonly Mock<ISlackNotificationsSender> _slackNotificationsSenderMock;

        public SlackAntispamTests()
        {
            _slackNotificationsSenderMock = new Mock<ISlackNotificationsSender>();
        }

        [Fact]
        public void TestSlackManagerPreventsSameMessageSpamWithDefaultConfiguration()
        {
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock.Object);
            manager.Start();
            var logEntry = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component", "Process", "Context", null, null, "Message", DateTime.UtcNow);

            // Act
            manager.SendNotification(logEntry);
            manager.SendNotification(logEntry);

            //Assert
            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == "Warning"), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TestSlackManagerPreventsDifferentMessageSpamWithDefaultConfiguration()
        {
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock.Object);
            manager.Start();

            // Act
            var logEntry1 = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component", "Process", "Context", null, null, "Message1", DateTime.UtcNow);
            manager.SendNotification(logEntry1);
            var logEntry2 = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component", "Process", "Context", null, null, "Message1", DateTime.UtcNow);
            manager.SendNotification(logEntry2);

            //Assert
            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == "Warning"), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TestSlackManagerAllowsDifferentProcessMessagesWithDefaultConfiguration()
        {
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock.Object);
            manager.Start();

            // Act
            var logEntry1 = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component", "Process1", "Context", null, null, "Message", DateTime.UtcNow);
            manager.SendNotification(logEntry1);
            var logEntry2 = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component", "Process2", "Context", null, null, "Message", DateTime.UtcNow);
            manager.SendNotification(logEntry2);

            //Assert
            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == "Warning"), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public void TestSlackManagerAllowsDifferentComponentMessagesWithDefaultConfiguration()
        {
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock.Object);
            manager.Start();

            // Act
            var logEntry1 = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component1", "Process", "Context", null, null, "Message", DateTime.UtcNow);
            manager.SendNotification(logEntry1);
            var logEntry2 = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component2", "Process", "Context", null, null, "Message", DateTime.UtcNow);
            manager.SendNotification(logEntry2);

            //Assert
            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == "Warning"), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public void TestSlackManagerAllowsSameMessageSpamAfterTimeoutWithDefaultConfiguration()
        {
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock.Object);
            manager.Start();
            var logEntry = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component", "Process", "Context", null, null, "Message", DateTime.UtcNow);

            // Act
            manager.SendNotification(logEntry);
            Thread.Sleep(TimeSpan.FromMinutes(1));
            manager.SendNotification(logEntry);

            //Assert
            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == "Warning"), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public void TestSlackManagerPreventsSameMessageSpamWithCustomConfiguration()
        {
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock.Object)
                .SetSpamMutePeriodForLevels(TimeSpan.FromSeconds(2), LogLevel.Warning);
            manager.Start();
            var logEntry = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component", "Process", "Context", null, null, "Message", DateTime.UtcNow);

            // Act
            manager.SendNotification(logEntry);
            manager.SendNotification(logEntry);

            //Assert
            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == "Warning"), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TestSlackManagerAllowsSameMessageSpamAfterTimeoutWithCustomConfiguration()
        {
            var mutePeriod = TimeSpan.FromSeconds(2);
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock.Object)
                .SetSpamMutePeriodForLevels(mutePeriod, LogLevel.Warning);
            manager.Start();
            var logEntry = LogEntity.CreateWithoutRowKey(
                LykkeLogToAzureStorage.WarningType, "Component", "Process", "Context", null, null, "Message", DateTime.UtcNow);

            // Act
            manager.SendNotification(logEntry);
            Thread.Sleep(mutePeriod);
            manager.SendNotification(logEntry);

            //Assert
            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == "Warning"), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public void TestLogToSlackPreventsSameMessageSpamWithDefaultConfiguration()
        {
            // Arrange
            string channel = "Prices";
            var logToSlack = LykkeLogToSlack.Create(_slackNotificationsSenderMock.Object, channel,  disableAntiSpam: false);

            // Act
            logToSlack.WriteInfoAsync("Process", "Context", "Message").GetAwaiter().GetResult();
            logToSlack.WriteInfoAsync("Process", "Context", "Message").GetAwaiter().GetResult();

            //Assert
            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == channel), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}

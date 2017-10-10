using System;
using Lykke.SlackNotifications;
using Moq;
using Xunit;

namespace Lykke.Logs.Tests
{
    public class LykkeLogToAzureStorageSlackNotificationsManagerTests
    {
        private readonly Mock<ISlackNotificationsSender> _slackNotificationsSenderMock;

        public LykkeLogToAzureStorageSlackNotificationsManagerTests()
        {
            _slackNotificationsSenderMock = new Mock<ISlackNotificationsSender>();
        }

        [Fact]
        public void Test_that_monitor_message_sends_to_the_slack()
        {
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock.Object);

            manager.Start();

            // Act
            manager.SendNotification(LogEntity.CreateWithoutRowKey(LykkeLogToAzureStorage.MonitorType, "Test", "", "", null, null, "Message", DateTime.UtcNow ));

            // Assert
            manager.Stop();

            _slackNotificationsSenderMock.Verify(x => x.SendAsync(It.Is<string>(t => t == "Monitor"), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
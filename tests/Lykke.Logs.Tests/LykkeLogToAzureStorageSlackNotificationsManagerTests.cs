using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Common.Log;
using Lykke.SlackNotifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lykke.Logs.Tests
{
    public class LykkeLogToAzureStorageSlackNotificationsManagerTests
    {
        private readonly ISlackNotificationsSender _slackNotificationsSenderMock;

        public LykkeLogToAzureStorageSlackNotificationsManagerTests()
        {
            _slackNotificationsSenderMock = Substitute.For<ISlackNotificationsSender>();
        }

        [Fact]
        public void Test_that_monitor_message_sends_to_the_slack()
        {
            // Arrange
            var manager = new LykkeLogToAzureSlackNotificationsManager(_slackNotificationsSenderMock);

            manager.Start();

            // Act
            manager.SendNotification(LogEntity.CreateWithoutRowKey(LykkeLogToAzureStorage.MonitorType, "Test", "", "", null, null, "Message", DateTime.UtcNow));

            // Assert
            manager.Stop();

            _slackNotificationsSenderMock.Received().SendAsync(Arg.Is<string>(t => t == "Monitor"), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
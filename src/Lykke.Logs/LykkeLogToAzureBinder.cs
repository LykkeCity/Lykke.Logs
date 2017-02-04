using AzureStorage.Tables;
using Common.Log;
using Lykke.SlackNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs
{

    public interface ILogToAzureSettings
    {
        string LogConnectionString { get; set; }
    }

    public static class LykkeLogToAzureBinder
    {
        public static ILog UseLogToAzureStorage(this IServiceCollection serviceCollection,
            ISlackNotificationsSender slackNotificationsSender,
            ILogToAzureSettings settings,
            string tableName = "Logs")
        {

            var applicationName =
                Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationName;

            var result = new LykkeLogToAzureStorage(
                applicationName,
                new AzureTableStorage<LogEntity>(settings.LogConnectionString, tableName, null),
                slackNotificationsSender);

            serviceCollection.AddSingleton<ILog>(result);

            return result;

        }
    }

}

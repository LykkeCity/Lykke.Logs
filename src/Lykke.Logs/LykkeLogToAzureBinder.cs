using AzureStorage.Tables;
using Common.Log;
using Lykke.SlackNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs
{

    public static class LykkeLogToAzureBinder
    {
        public static LykkeLogToAzureStorage UseLogToAzureStorage(this IServiceCollection serviceCollection,
            string  connectionString, 
            ISlackNotificationsSender slackNotificationsSender = null,
            string tableName = "Logs")
        {

            var applicationName =
                Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationName;

            var result = new LykkeLogToAzureStorage(
                applicationName,
                new AzureTableStorage<LogEntity>(connectionString, tableName, null),
                slackNotificationsSender);

            serviceCollection.AddSingleton<ILog>(result);

            return result;

        }
    }


}

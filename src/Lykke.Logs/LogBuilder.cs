using System;
using JetBrains.Annotations;
using Lykke.Logs.Loggers.LykkeAzureTable;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Logs.Loggers.LykkeSlack;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs
{
    internal class LogBuilder : ILogBuilder
    {
        public IServiceCollection Services { get; }
        public Action<ConsoleLoggerOptions> ConfigureConsole { get; set; }
        public Action<AzureTableLoggerOptions> ConfigureAzureTable { get; set; }
        public Action<SlackLoggerOptions> ConfigureEssentialSlackChannels { get; set; }

        public LogBuilder([NotNull] IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}
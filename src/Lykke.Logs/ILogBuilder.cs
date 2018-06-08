using JetBrains.Annotations;
using Lykke.Logs.Loggers.LykkeConsole;
using Microsoft.Extensions.DependencyInjection;
using System;
using Lykke.Logs.Loggers.LykkeAzureTable;
using Lykke.Logs.Loggers.LykkeSlack;

namespace Lykke.Logs
{
    /// <summary>
    /// Log builder
    /// </summary>
    [PublicAPI]
    public interface ILogBuilder
    {
        /// <summary>
        /// Services, which will be used to register logging services
        /// </summary>
        [NotNull]
        IServiceCollection Services { get; }

        /// <summary>
        /// Console logger configuration action. Optional.
        /// </summary>
        [CanBeNull]
        Action<ConsoleLoggerOptions> ConfigureConsole { get; set; }

        /// <summary>
        /// Azure table logger configuration action. Optional
        /// </summary>
        [CanBeNull]
        Action<AzureTableLoggerOptions> ConfigureAzureTable { get; set; }

        /// <summary>
        /// Essential slack channels configuration action. Optional.
        /// </summary>
        [CanBeNull]
        Action<SlackLoggerOptions> ConfigureEssentialSlackChannels { get; set; }
    }
}
using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    [PublicAPI]
    public static class LykkeConsoleLoggerExtensions
    {
        /// <summary>
        /// Adds a console logger named 'Console' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddLykkeConsole(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, LykkeConsoleLoggerProvider>();

            return builder;
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="Microsoft.Extensions.Logging.LogLevel"/>.Information or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        public static ILoggerFactory AddLykkeConsole(this ILoggerFactory factory)
        {
            return factory.AddLykkeConsole(includeScopes: false);
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="Microsoft.Extensions.Logging.LogLevel"/>.Information or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddLykkeConsole(this ILoggerFactory factory, bool includeScopes)
        {
            factory.AddLykkeConsole((n, l) => l >= Microsoft.Extensions.Logging.LogLevel.Information, includeScopes);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="Microsoft.Extensions.Logging.LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="minLevel">The minimum <see cref="Microsoft.Extensions.Logging.LogLevel"/> to be logged</param>
        public static ILoggerFactory AddLykkeConsole(this ILoggerFactory factory, Microsoft.Extensions.Logging.LogLevel minLevel)
        {
            factory.AddLykkeConsole(minLevel, includeScopes: false);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled for <see cref="Microsoft.Extensions.Logging.LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="minLevel">The minimum <see cref="Microsoft.Extensions.Logging.LogLevel"/> to be logged</param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddLykkeConsole(
            this ILoggerFactory factory,
            Microsoft.Extensions.Logging.LogLevel minLevel,
            bool includeScopes)
        {
            factory.AddLykkeConsole((category, logLevel) => logLevel >= minLevel, includeScopes);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled as defined by the filter function.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="filter">The category filter to apply to logs.</param>
        public static ILoggerFactory AddLykkeConsole(
            this ILoggerFactory factory,
            Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter)
        {
            factory.AddLykkeConsole(filter, includeScopes: false);
            return factory;
        }

        /// <summary>
        /// Adds a console logger that is enabled as defined by the filter function.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="filter">The category filter to apply to logs.</param>
        /// <param name="includeScopes">A value which indicates whether log scope information should be displayed
        /// in the output.</param>
        public static ILoggerFactory AddLykkeConsole(
            this ILoggerFactory factory,
            Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter,
            bool includeScopes)
        {
            var provider = new LykkeConsoleLoggerProvider(filter, includeScopes, ConsoleLogMessageWriter.Instance);

            factory.AddProvider(provider);

            return factory;
        }


        /// <summary>
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="settings">The settings to apply to created <see cref="LykkeConsoleLogger"/>'s.</param>
        /// <returns></returns>
        public static ILoggerFactory AddLykkeConsole(
            this ILoggerFactory factory,
            IConsoleLoggerSettings settings)
        {
            var provider = new LykkeConsoleLoggerProvider(settings, ConsoleLogMessageWriter.Instance);

            factory.AddProvider(provider);

            return factory;
        }

        /// <summary>
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to use for <see cref="IConsoleLoggerSettings"/>.</param>
        /// <returns></returns>
        public static ILoggerFactory AddLykkeConsole(this ILoggerFactory factory, IConfiguration configuration)
        {
            var settings = new ConfigurationConsoleLoggerSettings(configuration);
            return factory.AddLykkeConsole(settings);
        }
    }
}
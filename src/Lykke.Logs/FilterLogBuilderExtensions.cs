using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Logs
{
    [PublicAPI]
    public static class FilterLogBuilderExtensions
    {
        /// <summary>
        /// Sets minimal log level for the entire log system. Default minimum log level is <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/>
        /// </summary>
        public static ILogBuilder SetMinimumLevel(this ILogBuilder builder, Microsoft.Extensions.Logging.LogLevel level)
        {
            builder.Services.Add(ServiceDescriptor.Singleton<IConfigureOptions<LoggerFilterOptions>>(new DefaultLoggerLevelConfigureOptions(level)));
            
            return builder;
        }

        /// <summary>
        /// Adds filter, which could filter out log entries by logger provider name, component name and log level.
        /// </summary>
        public static ILogBuilder AddFilter(this ILogBuilder builder, Func<string, string, Microsoft.Extensions.Logging.LogLevel, bool> filter)
        {
            return builder.ConfigureFilter(options => options.AddFilter(filter));
        }

        /// <summary>
        /// Adds filter, which could filter out log entries by component name and log level.
        /// </summary>
        public static ILogBuilder AddFilter(this ILogBuilder builder, Func<string, Microsoft.Extensions.Logging.LogLevel, bool> componentLevelFilter)
        {
            return builder.ConfigureFilter(options => options.AddFilter(componentLevelFilter));
        }

        /// <summary>
        /// Add filter, which could filter out log entries by log level.
        /// </summary>
        public static ILogBuilder AddFilter(this ILogBuilder builder, Func<Microsoft.Extensions.Logging.LogLevel, bool> levelFilter)
        {
            return builder.ConfigureFilter(options => options.AddFilter(levelFilter));
        }

        /// <summary>
        /// Add filter, which could filter out log entries by minimal log level for specified component name.
        /// </summary>
        public static ILogBuilder AddFilter(this ILogBuilder builder, string componentName, Microsoft.Extensions.Logging.LogLevel minLevel)
        {
            return builder.ConfigureFilter(options => options.AddFilter(componentName, minLevel));
        }

        /// <summary>
        /// Add filter, which could filter out log entries by log level for specified component name.
        /// </summary>
        public static ILogBuilder AddFilter(this ILogBuilder builder, string componentName, Func<Microsoft.Extensions.Logging.LogLevel, bool> levelFilter)
        {
            return builder.ConfigureFilter(options => options.AddFilter(componentName, levelFilter));
        }

        private static ILogBuilder ConfigureFilter(this ILogBuilder builder, Action<LoggerFilterOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}
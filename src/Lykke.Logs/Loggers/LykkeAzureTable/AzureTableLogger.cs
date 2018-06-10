using System;
using AsyncFriendlyStackTrace;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    internal sealed class AzureTableLogger : ILogger
    {
        [NotNull] private readonly string _componentName;
        [NotNull] private readonly IAzureTableLogPersistenceQueue _persistenceQueue;

        public AzureTableLogger(
            [NotNull] string componentName, 
            [NotNull] IAzureTableLogPersistenceQueue persistenceQueue)
        {
            _componentName = componentName ?? throw new ArgumentNullException(nameof(componentName));
            _persistenceQueue = persistenceQueue ?? throw new ArgumentNullException(nameof(persistenceQueue));
        }

        void ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var parameters = state as LogEntryParameters ?? new ExternalLogEntryPerameters();

            var entry = LogEntity.CreateWithoutRowKey(
                parameters.AppName,
                parameters.AppVersion,
                parameters.EnvInfo,
                GetLogLevelString(logLevel),
                _componentName, 
                parameters.Process, 
                parameters.Context, 
                exception?.GetType().ToString(), 
                exception?.ToAsyncString(), 
                formatter(state, exception),
                parameters.Moment);

            _persistenceQueue.Enqueue(entry);
        }

        bool ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return true;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        private static string GetLogLevelString(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return "trace";
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return "debug";
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return "info";
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return "warning";
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return "error";
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return "critical";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
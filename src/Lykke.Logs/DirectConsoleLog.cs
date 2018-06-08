using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    internal sealed class DirectConsoleLog : ILog
    {
        private readonly ILog _log;

        public DirectConsoleLog(string componentName, ConsoleLoggerOptions options = null)
        {
            var logger = new LykkeConsoleLogger(
                componentName, 
                ConsoleLogMessageWriter.Instance,
                options ?? new ConsoleLoggerOptions());

            _log = new Log(logger, ConsoleHealthNotifier.Instance);
        }

        void ILog.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _log.Log(logLevel, eventId, state, exception, formatter);
        }

        bool ILog.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return _log.IsEnabled(logLevel);
        }

        IDisposable ILog.BeginScope(string scopeMessage)
        {
            return _log.BeginScope(scopeMessage);
        }

        #region Obsolete methods

        Task ILog.WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteInfoAsync(component, process, context, info, dateTime);
        }

        Task ILog.WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteMonitorAsync(component, process, context, info, dateTime);
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteWarningAsync(component, process, context, info, dateTime);
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            return _log.WriteWarningAsync(component, process, context, info, ex, dateTime);
        }

        Task ILog.WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            return _log.WriteErrorAsync(component, process, context, exception, dateTime);
        }

        Task ILog.WriteFatalErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            return _log.WriteFatalErrorAsync(component, process, context, exception, dateTime);
        }

        Task ILog.WriteInfoAsync(string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteInfoAsync(process, context, info, dateTime);
        }

        Task ILog.WriteMonitorAsync(string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteMonitorAsync(process, context, info, dateTime);
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteWarningAsync(process, context, info, dateTime);
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            return _log.WriteWarningAsync(process, context, info, ex, dateTime);
        }

        Task ILog.WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            return _log.WriteErrorAsync(process, context, exception, dateTime);
        }

        Task ILog.WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            return _log.WriteFatalErrorAsync(process, context, exception, dateTime);
        }

        #endregion
    }
}
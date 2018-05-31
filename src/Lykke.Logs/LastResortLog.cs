using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    internal sealed class LastResortLog : ILog
    {
        private readonly ILog _log;

        public LastResortLog(string componentName)
        {
            var logger = new LykkeConsoleLogger(componentName, (s, l) => true, true);

            _log = new Log(logger, new EmptyHealthNotifier());
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

        #region Not implemented obsolete methods

        Task ILog.WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteInfoAsync(string process, string context, string info, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteMonitorAsync(string process, string context, string info, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        Task ILog.WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
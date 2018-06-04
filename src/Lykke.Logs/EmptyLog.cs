using System;
using System.Threading.Tasks;
using Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace Lykke.Logs
{
    internal class EmptyLog : ILog
    {
        public static EmptyLog Instance { get; } = new EmptyLog();

        void ILog.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }

        bool ILog.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return false;
        }

        IDisposable ILog.BeginScope(string scopeMessage)
        {
            return NullScope.Instance;
        }

        #region Not implemented obsolete methods

        Task ILog.WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteInfoAsync(string process, string context, string info, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteMonitorAsync(string process, string context, string info, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        Task ILog.WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
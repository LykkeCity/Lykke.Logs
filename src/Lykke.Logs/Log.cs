using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    internal sealed class Log : ILog
    {
        private readonly ILogger _logger;
        private readonly IHealthNotifier _healthNotifier;

        public Log(ILogger logger, IHealthNotifier healthNotifier)
        {
            _logger = logger;
            _healthNotifier = healthNotifier;
        }

        void ILog.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        bool ILog.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        IDisposable ILog.BeginScope(string scopeMessage)
        {
            return _logger.BeginScope(scopeMessage);
        }

        #region Obsolete methods

        Task ILog.WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            this.Info(process, info, context, moment: dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            _healthNotifier.NotifyAsync(info, context);

            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            this.Warning(process, info, context: context, moment: dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, Exception ex,
            DateTime? dateTime)
        {
            this.Warning(process, info, ex, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            this.Error(process, "", exception, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime)
        {
            this.Critical(process, "", exception, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteInfoAsync(string process, string context, string info, DateTime? dateTime)
        {
            this.Info(process, info, context, moment: dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteMonitorAsync(string process, string context, string info, DateTime? dateTime)
        {
            _healthNotifier.NotifyAsync(info, context);

            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, DateTime? dateTime)
        {
            this.Warning(process, info, context: context, moment: dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            this.Warning(process, info, ex, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            this.Error(process, "", exception, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            this.Critical(process, "", exception, context, dateTime);

            return Task.CompletedTask;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    /// <summary>
    /// ILog decorator for sanitizing log data.
    /// </summary>
    public class SanitizingLog : ILog
    {
        private readonly ILog _log;
        private readonly List<(Regex pattern, string replacement)> _patterns = new List<(Regex, string)>();

        /// <summary>
        /// Initializes new instance.
        /// </summary>
        /// <param name="log">Original log instance.</param>
        public SanitizingLog(ILog log) => this._log = log ?? throw new System.ArgumentNullException(nameof(log));

        /// <summary>
        /// Adds sensitive pattern that should not be logged. Api keys, private keys and so on.
        /// </summary>
        /// <param name="pattern">Regex that should be replaced.</param>
        /// <param name="replacement">Pattern replacement.</param>
        public SanitizingLog AddSensitivePattern(Regex pattern, string replacement)
        {
            _patterns.Add((pattern, replacement));
            return this;
        }

        /// <summary>
        /// Replaces all patterns provided through <see cref="AddSensitivePattern(Regex, string)" /> with corresponding values.
        /// </summary>
        /// <param name="value">String to sanitize.</param>
        /// <returns></returns>
        public string Sanitize(string value) => _patterns.Aggregate(value, (a, p) => p.pattern.Replace(a, p.replacement));

        IDisposable ILog.BeginScope(string scopeMessage)
        {
            return _log.BeginScope(scopeMessage);
        }

        bool ILog.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return _log.IsEnabled(logLevel);
        }

        void ILog.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var sanitizedException = exception as SanitizingException ?? new SanitizingException(exception, Sanitize);
            var sanitizedState = new LogEntryParameters(
                state.AppName,
                state.AppVersion,
                state.EnvInfo,
                Sanitize(state.CallerFilePath),
                Sanitize(state.Process),
                state.CallerLineNumber,
                Sanitize(state.Message),
                Sanitize(state.Context),
                state.Moment);

            _log.Log(logLevel, eventId, sanitizedState, sanitizedException,
                (s, e) => Sanitize(formatter(state, exception)));
        }

        Task ILog.WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            return _log.WriteErrorAsync(
                Sanitize(component),
                Sanitize(process), 
                Sanitize(context), 
                new SanitizingException(exception, Sanitize),
                dateTime);
        }

        Task ILog.WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            return _log.WriteErrorAsync(
                Sanitize(process),
                Sanitize(context),
                new SanitizingException(exception, Sanitize),
                dateTime);
        }

        Task ILog.WriteFatalErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            return _log.WriteFatalErrorAsync(
                Sanitize(component),
                Sanitize(process),
                Sanitize(context),
                new SanitizingException(exception, Sanitize),
                dateTime);
        }

        Task ILog.WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            return _log.WriteFatalErrorAsync(
                Sanitize(process),
                Sanitize(context),
                new SanitizingException(exception, Sanitize),
                dateTime);
        }

        Task ILog.WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteInfoAsync(
                Sanitize(component),
                Sanitize(process),
                Sanitize(context),
                Sanitize(info),
                dateTime);
        }

        Task ILog.WriteInfoAsync(string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteInfoAsync(
                Sanitize(process),
                Sanitize(context),
                Sanitize(info),
                dateTime);
        }

        Task ILog.WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteMonitorAsync(
                Sanitize(component),
                Sanitize(process),
                Sanitize(context),
                Sanitize(info),
                dateTime);
        }

        Task ILog.WriteMonitorAsync(string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteMonitorAsync(
                Sanitize(process),
                Sanitize(context),
                Sanitize(info),
                dateTime);
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteWarningAsync(
                Sanitize(component),
                Sanitize(process),
                Sanitize(context),
                Sanitize(info),
                dateTime);
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            return _log.WriteWarningAsync(
                Sanitize(component),
                Sanitize(process),
                Sanitize(context),
                Sanitize(info),
                new SanitizingException(ex, Sanitize),
                dateTime);
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, DateTime? dateTime)
        {
            return _log.WriteWarningAsync(
                Sanitize(process),
                Sanitize(context),
                Sanitize(info),
                dateTime);
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            return _log.WriteWarningAsync(
                Sanitize(process),
                Sanitize(context),
                Sanitize(info),
                new SanitizingException(ex, Sanitize),
                dateTime);
        }
    }
}
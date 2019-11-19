using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeSanitizing
{
    internal sealed class SanitizingLog : ISanitizingLog
    {
        private readonly ILog _log;
        private readonly SanitizingOptions _options;

        public SanitizingLog(ILog log, SanitizingOptions options = null)
        {
            _log = log ?? throw new System.ArgumentNullException(nameof(log));
            _options = options ?? new SanitizingOptions();
        }

        public ISanitizingLog AddSanitizingFilter(Regex pattern, string replacement)
        {
            _options.Filters.Add(new SanitizingFilter(pattern, replacement));
            return this;
        }

        public string Sanitize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? value
                : _options.Filters.Aggregate(value, (a, p) => p.Pattern.Replace(a, p.Replacement));
        }

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
            var sanitizedFormatter = formatter != null ?
                new Func<LogEntryParameters, Exception, string>((s, e) => Sanitize(formatter(state, exception))) :
                null;

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

            _log.Log(logLevel, eventId, sanitizedState, exception, sanitizedFormatter);
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
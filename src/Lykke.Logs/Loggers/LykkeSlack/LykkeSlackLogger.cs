using System;
using System.Text;
using AsyncFriendlyStackTrace;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    internal sealed class LykkeSlackLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Func<Microsoft.Extensions.Logging.LogLevel, string> _channelResolver;
        private readonly ISlackLogEntriesSender _sender;

        public LykkeSlackLogger(
            [NotNull] ISlackLogEntriesSender sender,
            [NotNull] string categoryName,
            [NotNull] Func<Microsoft.Extensions.Logging.LogLevel, string> channelResolver)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(categoryName);
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _channelResolver = channelResolver ?? throw new ArgumentNullException(nameof(channelResolver));
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var channel = _channelResolver(logLevel);
            if (channel == null)
            {
                return;
            }

            var parameters = state as LogEntryParameters ?? new ExternalLogEntryPerameters();

            _sender.SendAsync(
                    logLevel,
                    parameters.Moment,
                    channel,
                    GetSender(logLevel, parameters),
                    BuildMessage(parameters, exception, formatter(state, exception)))
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return _channelResolver(logLevel) != null;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        private static string BuildMessage(LogEntryParameters parameters, Exception exception, string formattedMessage)
        {
            var sb = new StringBuilder();

            if (formattedMessage != null)
            {
                sb.Append(formattedMessage);
            }

            if (exception != null)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(exception.ToAsyncString());
            }

            if (parameters.Context != null)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(parameters.Context);
            }

            return sb.ToString();
        }

        private string GetSender(Microsoft.Extensions.Logging.LogLevel logLevel, LogEntryParameters parameters)
        {
            var sb = new StringBuilder();

            sb.Append($"{GetLogLevelString(logLevel)} {parameters.AppName} {parameters.AppVersion}");

            if (!string.IsNullOrWhiteSpace(parameters.EnvInfo))
            {
                sb.Append($" : {parameters.EnvInfo}");
            }

            if (_categoryName.StartsWith(parameters.AppName))
            {
                if (_categoryName.Length > parameters.AppName.Length)
                {
                    sb.Append($" : {_categoryName.Substring(parameters.AppName.Length)}");
                }
            }
            else
            {
                sb.Append($" : {_categoryName}");
            }

            return sb.ToString();
        }

        private static string GetLogLevelString(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return ":spiral_note_pad:";
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return ":computer:";
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return ":information_source:";
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return ":warning:";
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return ":exclamation:";
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return ":no_entry:";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
using System;
using System.Text;
using AsyncFriendlyStackTrace;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Console.Internal;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal sealed class LykkeConsoleLogger : ILogger
    {
        private readonly string _componentName;
        private readonly ConsoleLoggerOptions _options;
        private readonly IConsoleLogMessageWriter _writer;

        private static readonly string LogLevelPadding = ": ";
        private static readonly string MessagePadding;
        private static readonly string NewLineWithMessagePadding;
        
        // ConsoleColor does not have a value to specify the 'Default' color
        private readonly ConsoleColor? _defaultConsoleColor = null;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        static LykkeConsoleLogger()
        {
            var logLevelString = GetLogLevelString(Microsoft.Extensions.Logging.LogLevel.Information);
            MessagePadding = new string(' ', logLevelString.Length + LogLevelPadding.Length);
            NewLineWithMessagePadding = Environment.NewLine + MessagePadding;
        }

        public LykkeConsoleLogger(
            [NotNull] string componentName,
            [NotNull] IConsoleLogMessageWriter writer,
            [NotNull] ConsoleLoggerOptions options)
        {
            _componentName = componentName ?? throw new ArgumentNullException(nameof(componentName));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (!IsEnabled(logLevel))
            {
                return;
            }

            var callerInfo = state as LogEntryParameters ?? new ExternalLogEntryPerameters();

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, callerInfo, _componentName, message, exception);
            }
        }
        
        private void WriteMessage(Microsoft.Extensions.Logging.LogLevel logLevel, LogEntryParameters callerInfo, string logName, string message, Exception exception)
        {
            var logBuilder = _logBuilder;
            _logBuilder = null;

            if (logBuilder == null)
            {
                logBuilder = new StringBuilder();
            }

            var logLevelColors = GetLogLevelConsoleColors(logLevel);
            var logLevelString = GetLogLevelString(logLevel);

            logBuilder.Append(LogLevelPadding);
            logBuilder.AppendFormat(@"{0:MM-dd HH:mm:ss.fff}", callerInfo.Moment);
            logBuilder.Append(" : ");
            logBuilder.Append(logName);
            logBuilder.Append(" : ");
            logBuilder.AppendLine(callerInfo.Process);


            // scope information
            if (_options.IncludeScopes)
            {
                GetScopeInformation(logBuilder);
            }

            if (!string.IsNullOrEmpty(message))
            {
                // message
                logBuilder.Append(MessagePadding);

                var len = logBuilder.Length;
                logBuilder.AppendLine(message);
                logBuilder.Replace(Environment.NewLine, NewLineWithMessagePadding, len, message.Length);
            }

            var contextString = callerInfo.Context;
            if (!string.IsNullOrWhiteSpace(contextString))
            {
                var len = logBuilder.Length;
                logBuilder.Append(MessagePadding);
                logBuilder.AppendLine(contextString);
                logBuilder.Replace(Environment.NewLine, NewLineWithMessagePadding, len, contextString.Length + MessagePadding.Length);
            }

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception != null)
            {
                logBuilder.AppendLine(exception.ToAsyncString());
            }

            if (logBuilder.Length > 0)
            {
                var hasLevel = !string.IsNullOrEmpty(logLevelString);
                // Queue log message
                _writer.Write(new LogMessageEntry
                {
                    Message = logBuilder.ToString(),
                    MessageColor = _defaultConsoleColor,
                    LevelString = hasLevel ? logLevelString : null,
                    LevelBackground = hasLevel ? logLevelColors.Background : null,
                    LevelForeground = hasLevel ? logLevelColors.Foreground : null
                });
            }

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return ConsoleLogScope.Push(_componentName, state);
        }

        private static string GetLogLevelString(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return "TRACE";
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return "DEBUG";
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return "INFO";
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return "WARNING";
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return "ERROR";
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return "CRITICAL";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private ConsoleColors GetLogLevelConsoleColors(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            // We must explicitly set the background color if we are setting the foreground color,
            // since just setting one can look bad on the users console.
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return new ConsoleColors(ConsoleColor.White, ConsoleColor.Red);
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return new ConsoleColors(ConsoleColor.Red, null);
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return new ConsoleColors(ConsoleColor.Yellow, null);
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return new ConsoleColors(ConsoleColor.Gray, null);
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return new ConsoleColors(ConsoleColor.White, null);
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return new ConsoleColors(ConsoleColor.White, null);
                default:
                    return new ConsoleColors(_defaultConsoleColor, _defaultConsoleColor);
            }
        }


        private static void GetScopeInformation(StringBuilder builder)
        {
            var current = ConsoleLogScope.Current;
            var length = builder.Length;

            while (current != null)
            {
                var scopeLog = length == builder.Length
                    ? $"=> {current}"
                    : $"=> {current} ";

                builder.Insert(length, scopeLog);
                current = current.Parent;
            }
            if (builder.Length > length)
            {
                builder.Insert(length, MessagePadding);
                builder.AppendLine();
            }
        }

        private struct ConsoleColors
        {
            public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
            {
                Foreground = foreground;
                Background = background;
            }

            public ConsoleColor? Foreground { get; }

            public ConsoleColor? Background { get; }
        }
    }
}
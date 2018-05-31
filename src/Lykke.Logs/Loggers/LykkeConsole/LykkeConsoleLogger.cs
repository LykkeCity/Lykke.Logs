using System;
using System.Runtime.InteropServices;
using System.Text;
using AsyncFriendlyStackTrace;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Console.Internal;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal sealed class LykkeConsoleLogger : ILogger
    {
        private static readonly string LogLevelPadding = ": ";
        private static readonly string MessagePadding;
        private static readonly string NewLineWithMessagePadding;


        // ConsoleColor does not have a value to specify the 'Default' color
        private readonly ConsoleColor? _defaultConsoleColor = null;

        private readonly ConsoleLoggerProcessor _queueProcessor;
        private Func<string, Microsoft.Extensions.Logging.LogLevel, bool> _filter;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        static LykkeConsoleLogger()
        {
            var logLevelString = GetLogLevelString(Microsoft.Extensions.Logging.LogLevel.Information);
            MessagePadding = new string(' ', logLevelString.Length + LogLevelPadding.Length);
            NewLineWithMessagePadding = Environment.NewLine + MessagePadding;

        }

        public LykkeConsoleLogger(string name, Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter, bool includeScopes)
            : this(name, filter, includeScopes, new ConsoleLoggerProcessor())
        {
        }

        internal LykkeConsoleLogger(string name, Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter, bool includeScopes, ConsoleLoggerProcessor loggerProcessor)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Filter = filter ?? ((category, logLevel) => true);
            IncludeScopes = includeScopes;

            _queueProcessor = loggerProcessor;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console = new WindowsLogConsole();
            }
            else
            {
                Console = new AnsiLogConsole(new AnsiSystemConsole());
            }
        }

        public IConsole Console
        {
            get => _queueProcessor.Console;
            set => _queueProcessor.Console = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Func<string, Microsoft.Extensions.Logging.LogLevel, bool> Filter
        {
            get => _filter;
            set => _filter = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool IncludeScopes { get; set; }

        public string Name { get; }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var callerInfo = state as LogEntryParameters;
            if (callerInfo == null)
            {
                throw new ArgumentNullException(nameof(state), "Expected an argument state with a type assignable to LogEntryParameters");
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, callerInfo, Name, message, exception);
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
            if (IncludeScopes)
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
                _queueProcessor.EnqueueMessage(new LogMessageEntry
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
            return Filter(Name, logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return ConsoleLogScope.Push(Name, state);
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
                string scopeLog;
                if (length == builder.Length)
                {
                    scopeLog = $"=> {current}";
                }
                else
                {
                    scopeLog = $"=> {current} ";
                }

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

        private class AnsiSystemConsole : IAnsiSystemConsole
        {
            public void Write(string message)
            {
                System.Console.Write(message);
            }

            public void WriteLine(string message)
            {
                System.Console.WriteLine(message);
            }
        }
    }
}
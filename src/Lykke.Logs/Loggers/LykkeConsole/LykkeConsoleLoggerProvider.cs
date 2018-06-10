using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    /// <inheritdoc />
    [ProviderAlias("Console")]
    internal sealed class LykkeConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConsoleLoggerOptions _options;

        private readonly IConsoleLogMessageWriter _messageWriter;
        private readonly ConcurrentDictionary<string, LykkeConsoleLogger> _loggers;

        public LykkeConsoleLoggerProvider(
            [NotNull] ConsoleLoggerOptions options,
            [NotNull] IConsoleLogMessageWriter messageWriter)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));

            _loggers = new ConcurrentDictionary<string, LykkeConsoleLogger>();
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerImplementation);
        }

        private LykkeConsoleLogger CreateLoggerImplementation(string name)
        {
            return new LykkeConsoleLogger(name, _messageWriter, _options);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _messageWriter.Dispose();
        }
    }
}

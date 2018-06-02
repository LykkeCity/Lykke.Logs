using System;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    internal sealed class SpamGuardingLoggerDecorator : ILogger
    {
        private readonly string _componentName;
        private readonly ILogger _logger;
        private readonly ISpamGuard<Microsoft.Extensions.Logging.LogLevel> _spamGuard;

        public SpamGuardingLoggerDecorator(
            [NotNull] string categoryName,
            [NotNull] ILogger logger,
            [NotNull] ISpamGuard<Microsoft.Extensions.Logging.LogLevel> spamGuard)
        {
            _componentName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _spamGuard = spamGuard ?? throw new ArgumentNullException(nameof(spamGuard));
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var parameters = state as LogEntryParameters ?? new ExternalLogEntryPerameters();

            if (_spamGuard.ShouldBeMutedAsync(logLevel, _componentName, parameters.Process).ConfigureAwait(false).GetAwaiter().GetResult())
            {
                return;
            }

            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }
    }
}

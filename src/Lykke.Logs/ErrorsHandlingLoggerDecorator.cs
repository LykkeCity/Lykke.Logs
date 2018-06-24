using System;
using AsyncFriendlyStackTrace;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace Lykke.Logs
{
    internal sealed class ErrorsHandlingLoggerDecorator : ILogger
    {
        [NotNull] private readonly ILogger _logger;

        public ErrorsHandlingLoggerDecorator([NotNull] ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                _logger.Log(logLevel, eventId, state, exception, formatter);
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine(ex.ToAsyncString());
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }
        }
        
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            try
            {
                return _logger.IsEnabled(logLevel);
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine(ex.ToAsyncString());
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }

            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            try
            {
                return _logger.BeginScope(state);
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine(ex.ToAsyncString());
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }

            return NullScope.Instance;
        }
    }
}
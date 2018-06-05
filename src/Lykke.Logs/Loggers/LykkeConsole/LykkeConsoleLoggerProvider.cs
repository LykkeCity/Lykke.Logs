using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    /// <inheritdoc />
    [ProviderAlias("Console")]
    internal sealed class LykkeConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, LykkeConsoleLogger> _loggers = new ConcurrentDictionary<string, LykkeConsoleLogger>();

        private readonly Func<string, Microsoft.Extensions.Logging.LogLevel, bool> _filter;
        private readonly bool _includeScopes;

        private readonly IConsoleLogMessageWriter _messageWriter;

        private IConsoleLoggerSettings _settings;
        
        private static readonly Func<string, Microsoft.Extensions.Logging.LogLevel, bool> FalseFilter = (cat, level) => false;
        
        public LykkeConsoleLoggerProvider(
            [NotNull] Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter, 
            bool includeScopes,
            [NotNull] IConsoleLogMessageWriter writer)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _includeScopes = includeScopes;

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            _messageWriter = new BufferedConsoleLogMessageWriterDecorator(writer);
        }

        public LykkeConsoleLoggerProvider(
            [NotNull] IConsoleLoggerSettings settings,
            [NotNull] IConsoleLogMessageWriter writer)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _settings.ChangeToken?.RegisterChangeCallback(OnConfigurationReload, null);

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            _messageWriter = new BufferedConsoleLogMessageWriterDecorator(writer);
        }

        private void OnConfigurationReload(object state)
        {
            try
            {
                // The settings object needs to change here, because the old one is probably holding on
                // to an old change token.
                _settings = _settings.Reload();

                var includeScopes = _settings?.IncludeScopes ?? false;
                foreach (var logger in _loggers.Values)
                {
                    logger.Filter = GetFilter(logger.Name, _settings);
                    logger.IncludeScopes = includeScopes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while loading configuration changes.{Environment.NewLine}{ex}");
            }
            finally
            {
                // The token will change each time it reloads, so we need to register again.
                _settings?.ChangeToken?.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerImplementation);
        }

        private LykkeConsoleLogger CreateLoggerImplementation(string name)
        {
            var includeScopes = _settings?.IncludeScopes ?? _includeScopes;

            return new LykkeConsoleLogger(name, GetFilter(name, _settings), includeScopes, _messageWriter);
        }

        private Func<string, Microsoft.Extensions.Logging.LogLevel, bool> GetFilter(string name, IConsoleLoggerSettings settings)
        {
            if (_filter != null)
            {
                return _filter;
            }

            if (settings != null)
            {
                foreach (var prefix in GetKeyPrefixes(name))
                {
                    if (settings.TryGetSwitch(prefix, out var level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return FalseFilter;
        }

        private IEnumerable<string> GetKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                var lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _messageWriter.Dispose();
        }
    }
}

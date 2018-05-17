using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Console.Internal;
using Microsoft.Extensions.Options;

namespace Lykke.Logs
{
    [ProviderAlias("Console")]
    public sealed class LykkeConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, LykkeConsoleLogger> _loggers = new ConcurrentDictionary<string, LykkeConsoleLogger>();

        private readonly Func<string, Microsoft.Extensions.Logging.LogLevel, bool> _filter;
        private IConsoleLoggerSettings _settings;
        private readonly LykkeConsoleLoggerProcessor _messageQueue = new LykkeConsoleLoggerProcessor();

        private static readonly Func<string, Microsoft.Extensions.Logging.LogLevel, bool> TrueFilter = (cat, level) => true;
        private static readonly Func<string, Microsoft.Extensions.Logging.LogLevel, bool> FalseFilter = (cat, level) => false;
        private IDisposable _optionsReloadToken;
        private bool _includeScopes;

        public LykkeConsoleLoggerProvider(Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter, bool includeScopes)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _filter = filter;
            _includeScopes = includeScopes;
        }

        public LykkeConsoleLoggerProvider(IOptionsMonitor<ConsoleLoggerOptions> options)
        {
            // Filter would be applied on LoggerFactory level
            _filter = TrueFilter;
            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            ReloadLoggerOptions(options.CurrentValue);
        }

        private void ReloadLoggerOptions(ConsoleLoggerOptions options)
        {
            _includeScopes = options.IncludeScopes;
            foreach (var logger in _loggers.Values)
            {
                logger.IncludeScopes = _includeScopes;
            }
        }

        public LykkeConsoleLoggerProvider(IConsoleLoggerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _settings.ChangeToken?.RegisterChangeCallback(OnConfigurationReload, null);
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
                System.Console.WriteLine($"Error while loading configuration changes.{Environment.NewLine}{ex}");
            }
            finally
            {
                // The token will change each time it reloads, so we need to register again.
                _settings?.ChangeToken?.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }

        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerImplementation);
        }

        private LykkeConsoleLogger CreateLoggerImplementation(string name)
        {
            var includeScopes = _settings?.IncludeScopes ?? _includeScopes;
            return new LykkeConsoleLogger(name, GetFilter(name, _settings), includeScopes, _messageQueue);
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

        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
            _messageQueue.Dispose();
        }
    }
}

﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Logs.Loggers.LykkeSanitizing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Logs
{
    /// <summary>
    /// Log factory
    /// </summary>
    [PublicAPI]
    public sealed class LogFactory : ILogFactory
    {
        internal static ILogFactory LastResort { get; } = Create().AddUnbufferedConsole();

        private readonly ILoggerFactory _loggerFactory;
        private readonly Func<IHealthNotifier> _healthNotifierProvider;
        private readonly SanitizingOptions _sanitizingOptions;

        internal LogFactory(ILoggerFactory loggerFactory, Func<IHealthNotifier> healthNotifierProvider, IOptions<SanitizingOptions> sanitizingOptions = null)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _healthNotifierProvider = healthNotifierProvider ?? throw new ArgumentNullException(nameof(healthNotifierProvider));
            _sanitizingOptions = sanitizingOptions?.Value ?? new SanitizingOptions();
        }

        /// <summary>
        /// Creates empty log factory
        /// </summary>
        public static ILogFactory Create()
        {
            return new LogFactory(
                new LoggerFactory(),
                () => NotSupportedHealthNotifier.Instance);
        }

        /// <inheritdoc />
        public ILog CreateLog<TComponent>(TComponent component, string componentNameSuffix)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            if (string.IsNullOrWhiteSpace(componentNameSuffix))
            {
                throw new ArgumentException("Should be not empty string", nameof(componentNameSuffix));
            }

            ILog log = new Log(_loggerFactory.CreateLogger(ComponentNameHelper.GetComponentName(component, componentNameSuffix)), _healthNotifierProvider.Invoke());

            return _sanitizingOptions.Filters.Any()
                ? new SanitizingLog(log, _sanitizingOptions)
                : log;
        }

        /// <inheritdoc />
        public ILog CreateLog<TComponent>(TComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            ILog log = new Log(_loggerFactory.CreateLogger(ComponentNameHelper.GetComponentName(component)), _healthNotifierProvider.Invoke());

            return _sanitizingOptions.Filters.Any()
                ? new SanitizingLog(log, _sanitizingOptions)
                : log;
        }

        /// <inheritdoc />
        public void AddProvider(ILoggerProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _loggerFactory.AddProvider(provider);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _loggerFactory.Dispose();
        }
    }
}
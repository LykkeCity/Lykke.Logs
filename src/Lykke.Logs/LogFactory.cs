using System;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using Microsoft.Extensions.Logging;

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

        internal LogFactory(ILoggerFactory loggerFactory, Func<IHealthNotifier> healthNotifierProvider)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _healthNotifierProvider = healthNotifierProvider ?? throw new ArgumentNullException(nameof(healthNotifierProvider));
        }

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

            return new Log(_loggerFactory.CreateLogger(ComponentNameHelper.GetComponentName(component, componentNameSuffix)), _healthNotifierProvider.Invoke());
        }

        /// <inheritdoc />
        public ILog CreateLog<TComponent>(TComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            return new Log(_loggerFactory.CreateLogger(ComponentNameHelper.GetComponentName(component)), _healthNotifierProvider.Invoke());
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

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }
    }
}
using System;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace Lykke.Logs
{
    internal sealed class LogFactory : ILogFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly Func<IHealthNotifier> _healthNotifierProvider;

        public LogFactory(ILoggerFactory loggerFactory, Func<IHealthNotifier> healthNotifierProvider)
        {
            _loggerFactory = loggerFactory;
            _healthNotifierProvider = healthNotifierProvider;
        }

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

            return new Log(_loggerFactory.CreateLogger($"{TypeNameHelper.GetTypeDisplayName(component.GetType())}[{componentNameSuffix}]"), _healthNotifierProvider.Invoke());
        }

        public ILog CreateLog<TComponent>(TComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            return new Log(_loggerFactory.CreateLogger(TypeNameHelper.GetTypeDisplayName(component.GetType())), _healthNotifierProvider.Invoke());
        }
    }
}
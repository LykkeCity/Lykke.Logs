using System;
using System.Collections.Concurrent;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace Lykke.Logs
{
    /// <summary>
    /// Direct (not buffered) console log factory. It logs only to the console and can't be filtered out. 
    /// Could be used to log events of logging system, when more complex loggers faced with failure and in the tests
    /// </summary>
    [PublicAPI]
    public sealed class DirectConsoleLogFactory : ILogFactory
    {
        /// <summary>
        /// Instance of the last resort log factory
        /// </summary>
        [NotNull]
        public static ILogFactory Instance { get; } = new DirectConsoleLogFactory();

        private readonly ConcurrentDictionary<(Type Type, string Suffix), DirectConsoleLog> _logs;

        private DirectConsoleLogFactory()
        {
            _logs = new ConcurrentDictionary<(Type, string), DirectConsoleLog>();
        }

        public ILog CreateLog<TComponent>(TComponent component, string componentNameSuffix)
        {
            return _logs.GetOrAdd((component.GetType(), componentNameSuffix), (key) =>
            {
                var componentName = key.Suffix != null
                    ? $"{TypeNameHelper.GetTypeDisplayName(key.Type)}[{key.Suffix}]"
                    : TypeNameHelper.GetTypeDisplayName(key.Type);

                return new DirectConsoleLog(componentName);
            });
        }

        public ILog CreateLog<TComponent>(TComponent component)
        {
            return CreateLog(component, null);
        }
    }
}
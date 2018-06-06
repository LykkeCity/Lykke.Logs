using System.Collections.Concurrent;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;

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

        private readonly ConcurrentDictionary<string, DirectConsoleLog> _logs;

        private DirectConsoleLogFactory()
        {
            _logs = new ConcurrentDictionary<string, DirectConsoleLog>();
        }

        public ILog CreateLog<TComponent>(TComponent component, string componentNameSuffix)
        {
            var componentName = ComponentNameHelper.GetComponentName(component, componentNameSuffix);

            return _logs.GetOrAdd(componentName, key => new DirectConsoleLog(key));
        }

        public ILog CreateLog<TComponent>(TComponent component)
        {
            return CreateLog(component, null);
        }
    }
}
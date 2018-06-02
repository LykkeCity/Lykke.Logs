using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Logs
{
    /// <summary>
    /// Log factory, that creates empty log. Could be used in tests.
    /// You can pass <see cref="Instance"/> member, whenever you need to inject the log factory for testing purpose
    /// </summary>
    [PublicAPI]
    public sealed class EmptyLogFactory : ILogFactory
    {
        /// <summary>
        /// Instance of the empty log factory
        /// </summary>
        public static ILogFactory Instance { get; } = new EmptyLogFactory();

        /// <inheritdoc />
        public ILog CreateLog<TComponent>(TComponent component, string componentNameSuffix)
        {
            return EmptyLog.Instance;
        }

        /// <inheritdoc />
        public ILog CreateLog<TComponent>(TComponent component)
        {
            return EmptyLog.Instance;
        }
    }
}
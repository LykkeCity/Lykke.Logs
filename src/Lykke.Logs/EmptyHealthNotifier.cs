using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Logs
{
    /// <summary>
    /// Health notifier, that notifies nobody. Could be used in tests
    /// </summary>
    [PublicAPI]
    public sealed class EmptyHealthNotifier : IHealthNotifier
    {
        public static IHealthNotifier Instance { get; } = new EmptyHealthNotifier();

        private EmptyHealthNotifier()
        {
        }

        public void Notify(string healthMessage, object context = null)
        {
        }

        public void Dispose()
        {
        }
    }
}
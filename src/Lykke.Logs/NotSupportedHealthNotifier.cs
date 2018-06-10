using System;
using Lykke.Common.Log;

namespace Lykke.Logs
{
    internal sealed class NotSupportedHealthNotifier : IHealthNotifier
    {
        public static IHealthNotifier Instance { get; } = new NotSupportedHealthNotifier();

        private NotSupportedHealthNotifier()
        {
        }

        public void Dispose()
        {
        }

        public void Notify(string healthMessage, object context = null)
        {
            throw new NotSupportedException("This opperation is not supported");
        }
    }
}
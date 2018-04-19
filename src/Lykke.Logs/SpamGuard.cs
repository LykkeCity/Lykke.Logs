using System;
using System.Collections.Concurrent;

namespace Lykke.Logs
{
    internal class SpamGuard
    {
        private struct LastMessageInfo
        {
            internal DateTime Time { get; set; }
            internal string Message { get; set; }
        }

        private readonly TimeSpan _sameMessageMutePeriod = TimeSpan.FromSeconds(60);
        private readonly ConcurrentDictionary<LogLevel, LastMessageInfo> _lastMessages = new ConcurrentDictionary<LogLevel, LastMessageInfo>();

        internal bool IsSameMessage(LogLevel level, string message)
        {
            var now = DateTime.UtcNow;
            bool isSameMessage = false;
            _lastMessages.AddOrUpdate(
                level,
                new LastMessageInfo { Time = now, Message = message },
                (l, c) =>
                {
                    if (now - c.Time < _sameMessageMutePeriod)
                    {
                        isSameMessage = true;
                        return c;
                    }
                    return new LastMessageInfo { Time = now, Message = message };
                });

            return isSameMessage;
        }
    }
}

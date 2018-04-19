using System;
using System.Collections.Concurrent;

namespace Lykke.Logs
{
    internal class SpamGuard
    {
        private class LastMessageInfo
        {
            internal DateTime Time { get; set; }
            internal string Message { get; set; }
        }

        private readonly TimeSpan _sameMessageMutePeriod = TimeSpan.FromSeconds(60);
        private readonly ConcurrentDictionary<LogLevel, LastMessageInfo> _lastMessages = new ConcurrentDictionary<LogLevel, LastMessageInfo>();

        internal bool IsSameMessage(LogLevel level, string message)
        {
            var now = DateTime.UtcNow;
            if (_lastMessages.TryGetValue(level, out LastMessageInfo lastMessage))
            {
                if (lastMessage.Message == message)
                {
                    if (now - lastMessage.Time < _sameMessageMutePeriod)
                        return true;
                }
                else
                {
                    _lastMessages.TryUpdate(level, new LastMessageInfo { Time = now, Message = message }, lastMessage);
                }
            }
            else
            {
                _lastMessages.TryAdd(level, new LastMessageInfo { Time = now, Message = message });
            }
            return false;
        }
    }
}

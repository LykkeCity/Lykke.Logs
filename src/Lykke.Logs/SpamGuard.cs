using System;
using System.Collections.Concurrent;

namespace Lykke.Logs
{
    internal class SpamGuard
    {
        private readonly TimeSpan _sameMessageMutePeriod = TimeSpan.FromSeconds(60);
        private readonly ConcurrentDictionary<LogLevel, DateTime> _lastTimes = new ConcurrentDictionary<LogLevel, DateTime>();
        private readonly ConcurrentDictionary<LogLevel, string> _lastMessages = new ConcurrentDictionary<LogLevel, string>();

        internal bool IsSameMessage(LogLevel level, string message)
        {
            var now = DateTime.UtcNow;
            if (_lastTimes.TryGetValue(level, out DateTime lastTime))
            {
                if (_lastMessages.TryGetValue(level, out string lastMessage))
                {
                    if (lastMessage == message)
                    {
                        if (now - lastTime < _sameMessageMutePeriod)
                            return true;
                    }
                    else
                    {
                        _lastMessages.TryUpdate(level, message, lastMessage);
                    }
                }
                else
                {
                    _lastMessages.TryAdd(level, message);
                }
                if (now != lastTime)
                    _lastTimes.TryUpdate(level, now, lastTime);
            }
            else
            {
                _lastTimes.TryAdd(level, now);
                _lastMessages.TryAdd(level, message);
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Lykke.Logs
{
    internal class SpamGuard
    {
        private struct LastMessageInfo
        {
            internal DateTime Time { get; set; }
            internal string Component { get; set; }
            internal string Process { get; set; }
            internal string Message { get; set; }
        }

        private readonly ConcurrentDictionary<LogLevel, LastMessageInfo> _lastMessages = new ConcurrentDictionary<LogLevel, LastMessageInfo>();
        private readonly Dictionary<LogLevel, TimeSpan> _mutePeriods = new Dictionary<LogLevel, TimeSpan>();

        internal void SetMutePeriod(LogLevel level, TimeSpan mutePeriod)
        {
            _mutePeriods[level] = mutePeriod;
        }

        internal bool IsSameMessage(
            LogLevel level,
            string component,
            string process,
            string message)
        {
            if (!_mutePeriods.ContainsKey(level))
                return false;

            var now = DateTime.UtcNow;
            bool isSameMessage = false;
            var messageInfo = new LastMessageInfo
            {
                Time = now,
                Component = component,
                Process = process,
                Message = message,
            };
            _lastMessages.AddOrUpdate(
                level,
                messageInfo,
                (l, c) =>
                {
                    if (c.Component == component
                        && c.Process == process
                        && c.Message == message
                        && now - c.Time < _mutePeriods[level])
                    {
                        isSameMessage = true;
                        return c;
                    }
                    return messageInfo;
                });

            return isSameMessage;
        }
    }
}

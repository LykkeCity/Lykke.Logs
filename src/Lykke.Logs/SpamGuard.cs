using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Lykke.Logs
{
    internal class SpamGuard : TimerPeriod
    {
        private readonly ConcurrentDictionary<LogLevel, Dictionary<string, DateTime>> _lastMessages =
            new ConcurrentDictionary<LogLevel, Dictionary<string, DateTime>>();
        private readonly Dictionary<LogLevel, TimeSpan> _mutePeriods = new Dictionary<LogLevel, TimeSpan>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private bool _disableGuarding;

        public SpamGuard()
            : base((int)TimeSpan.FromMinutes(5).TotalMilliseconds)
        {
        }

        internal void DisableGuarding()
        {
            _disableGuarding = true;
            _mutePeriods.Clear();
        }

        internal void SetMutePeriod(LogLevel level, TimeSpan mutePeriod)
        {
            if (_disableGuarding)
                throw new InvalidOperationException("AntiSpam protection is disabled");
            _mutePeriods[level] = mutePeriod;
        }

        internal async Task<bool> ShouldBeMutedAsync(
            LogLevel level,
            string component,
            string process,
            string message)
        {
            if (!_mutePeriods.ContainsKey(level))
                return false;

            var levelDict = _lastMessages.GetOrAdd(level, new Dictionary<string, DateTime>());
            var key = GetEntryKey(component, process);
            var now = DateTime.UtcNow;
            DateTime lastTime;
            await _lock.WaitAsync();
            try
            {
                levelDict.TryGetValue(key, out lastTime);
                levelDict[key] = now;
            }
            finally
            {
                _lock.Release();
            }
            return now - lastTime <= _mutePeriods[level];
        }

        private static string GetEntryKey(string component, string process)
        {
            return $"{component}_{process}";
        }

        public override async Task Execute()
        {
            var now = DateTime.UtcNow;
            foreach (var level in _lastMessages.Keys)
            {
                var levelDict = _lastMessages[level];
                var mutePeriod = _mutePeriods[level];
                await _lock.WaitAsync();
                try
                {
                    foreach (var key in levelDict.Keys)
                    {
                        var lastTime = levelDict[key];
                        if (now - lastTime > mutePeriod)
                            levelDict.Remove(key);
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }
}

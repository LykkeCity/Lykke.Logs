using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.Logs
{
    internal sealed class SpamGuard<TLevel> : 
        TimerPeriod, 
        ISpamGuardConfiguration<TLevel>, 
        ISpamGuard<TLevel>
    {
        private readonly ConcurrentDictionary<TLevel, Dictionary<string, DateTime>> _lastMessages =
            new ConcurrentDictionary<TLevel, Dictionary<string, DateTime>>();
        private readonly Dictionary<TLevel, TimeSpan> _mutePeriods = new Dictionary<TLevel, TimeSpan>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private bool _disableGuarding;

        [Obsolete]
        public SpamGuard(ILog log)
            : base((int)TimeSpan.FromMinutes(5).TotalMilliseconds, log)
        {
            DisableTelemetry();
        }

        public SpamGuard(ILogFactory lastResortLogFactory)
            : base(TimeSpan.FromMinutes(5), lastResortLogFactory)
        {
            DisableTelemetry();
        }

        public void DisableGuarding()
        {
            _disableGuarding = true;
            _mutePeriods.Clear();
        }

        public void SetMutePeriod(TLevel level, TimeSpan mutePeriod)
        {
            if (_disableGuarding)
                throw new InvalidOperationException("AntiSpam protection is disabled");
            _mutePeriods[level] = mutePeriod;
        }

        public override void Start()
        {
            if (_disableGuarding)
            {
                return;
            }

            base.Start();
        }

        public async Task<bool> ShouldBeMutedAsync(
            TLevel level,
            string component,
            string process)
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
                    var keys = new List<string>(levelDict.Keys);
                    foreach (var key in keys)
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

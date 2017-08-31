using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.AzureStorage;

namespace Lykke.Logs
{
    public class LykkeLogToAzureStorage : 
        TimerPeriod,
        ILog
    {
        public const string ErrorType = "error";
        public const string FatalErrorType = "fatalerror";
        public const string WarningType = "warning";

        private readonly ILykkeLogToAzureStoragePersistenceManager _persistenceManager;
        private ILykkeLogToAzureSlackNotificationsManager _slackNotificationsManager;
        private readonly ILog _lastResortLog;
        private readonly TimeSpan _maxBatchLifetime;
        private readonly int _batchSizeThreshold;
        private readonly bool _ownPersistenceManager;
        private readonly bool _ownSlackNotificationsManager;

        private List<LogEntity> _currentBatch;
        private volatile int _currentBatchSize;
        private int _maxBatchSize;
        private DateTime _currentBatchDeathtime;

        /// <param name="applicationName">Application name</param>
        /// <param name="persistenceManager">Persistence manager</param>
        /// <param name="slackNotificationsManager">Slack notifications manager. Can be null</param>
        /// <param name="lastResortLog">Last resort log (e.g. Console), which will be used to log logging infrastructure's issues</param>
        /// <param name="maxBatchLifetime">Log entries batch's lifetime, when exceeded, batch will be saved, and new batch will be started. Default is 5 seconds</param>
        /// <param name="batchSizeThreshold">Log messages batch's size threshold, when exceeded, batch will be saved, and new batch will be started. Default is 100 entries</param>
        /// <param name="ownPersistenceManager">Is log instance owns persistence manager: should it manages Start/Stop</param>
        /// <param name="ownSlackNotificationsManager">Is log instance owns slack notifications manager: should it manages Start/Stop</param>
        public LykkeLogToAzureStorage(
            string applicationName,
            ILykkeLogToAzureStoragePersistenceManager persistenceManager,
            ILykkeLogToAzureSlackNotificationsManager slackNotificationsManager = null,
            ILog lastResortLog = null,
            TimeSpan? maxBatchLifetime = null,
            int batchSizeThreshold = 100,
            bool ownPersistenceManager = true,
            bool ownSlackNotificationsManager = true) :

            base(applicationName, periodMs: 20, log: lastResortLog ?? new EmptyLog())
        {
            _persistenceManager = persistenceManager;
            _slackNotificationsManager = slackNotificationsManager;
            _lastResortLog = lastResortLog;
            _batchSizeThreshold = batchSizeThreshold;
            _ownPersistenceManager = ownPersistenceManager;
            _ownSlackNotificationsManager = ownSlackNotificationsManager;
            _maxBatchLifetime = maxBatchLifetime ?? TimeSpan.FromSeconds(5);

            StartNewBatch();
        }

        public override void Start()
        {
            StartNewBatch();

            if (_ownPersistenceManager)
            {
                (_persistenceManager as IStartable)?.Start();
            }
            if (_ownSlackNotificationsManager)
            {
                (_slackNotificationsManager as IStartable)?.Start();
            }

            base.Start();
        }

        public override void Stop()
        {
            if (_currentBatch != null)
            {
                lock (_currentBatch)
                {
                    _persistenceManager.Persist(_currentBatch);
                    _currentBatch = null;
                }
            }

            base.Stop();

            if (_ownPersistenceManager)
            {
                (_persistenceManager as IStopable)?.Stop();
            }
            if (_ownSlackNotificationsManager)
            {
                (_slackNotificationsManager as IStopable)?.Stop();
            }
        }

        public LykkeLogToAzureStorage SetSlackNotificationsManager(ILykkeLogToAzureSlackNotificationsManager notificationsManager)
        {
            _slackNotificationsManager = notificationsManager;

            return this;
        }

        public Task WriteInfoAsync(string component, string process, string context, string info,
            DateTime? dateTime = null)
        {
            return Insert("info", component, process, context, null, null, info, dateTime);
        }

        public Task WriteWarningAsync(string component, string process, string context, string info,
            DateTime? dateTime = null)
        {
            return Insert(WarningType, component, process, context, null, null, info, dateTime);
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            return Insert(ErrorType, component, process, context, exception.GetType().ToString(), exception.ToString(),
                GetExceptionMessage(exception), dateTime);
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            return Insert(FatalErrorType, component, process, context, exception.GetType().ToString(),
                exception.ToString(), GetExceptionMessage(exception), dateTime);
        }

        public override Task Execute()
        {
            if (_currentBatch == null)
            {
                return Task.CompletedTask;
            }

            IReadOnlyCollection<LogEntity> batchToSave = null;

            var now = DateTime.UtcNow;

            if (_currentBatchSize >= _batchSizeThreshold || _currentBatchSize > 0 && now >= _currentBatchDeathtime)
            {
                lock (_currentBatch)
                {
                    if (_currentBatchSize >= _batchSizeThreshold || _currentBatchSize > 0 && now >= _currentBatchDeathtime)
                    {
                        batchToSave = _currentBatch;
                        StartNewBatch();
                    }
                }
            }

            if (batchToSave != null)
            {
                _persistenceManager.Persist(batchToSave);
            }

            return Task.CompletedTask;
        }

        private Task Insert(string level, string component, string process, string context, string type, string stack,
            string msg, DateTime? dateTime)
        {
            try
            {
                if (_currentBatch == null)
                {
                    return Task.CompletedTask;
                }

                var dt = dateTime ?? DateTime.UtcNow;
                var newEntity = LogEntity.Create(level, component, process, context, type, stack, msg, dt);

                lock (_currentBatch)
                {
                    _currentBatch.Add(newEntity);
                    ++_currentBatchSize;
                }

                _slackNotificationsManager?.SendNotification(newEntity);
            }
            catch (Exception ex)
            {
                if (_lastResortLog != null)
                {
                    return _lastResortLog.WriteErrorAsync(nameof(LykkeLogToAzureStorage), nameof(Insert), "", ex);
                }
            }

            return Task.CompletedTask;
        }

        private void StartNewBatch()
        {
            _maxBatchSize = Math.Max(_maxBatchSize, _currentBatchSize);
            _currentBatch = new List<LogEntity>(_maxBatchSize);
            _currentBatchSize = 0;
            _currentBatchDeathtime = DateTime.UtcNow + _maxBatchLifetime;
        }

        private static string GetExceptionMessage(Exception exception)
        {
            var ex = exception;
            var sb = new StringBuilder();

            while (true)
            {
                sb.AppendLine(ex.Message);

                ex = ex.InnerException;

                if (ex == null)
                {
                    return sb.ToString();
                }

                sb.Append(" -> ");
            }
        }
    }
}

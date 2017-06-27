using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Log;
using Lykke.SlackNotifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs
{

    public class LogEntity : TableEntity
    {
        public static string GeneratePartitionKey(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }
        public DateTime DateTime { get; set; }
        public string Level { get; set; }
        public string Component { get; set; }
        public string Process { get; set; }
        public string Context { get; set; }
        public string Type { get; set; }
        public string Stack { get; set; }
        public string Msg { get; set; }

        public static LogEntity Create(string level, string component, string process, string context, string type, string stack, string msg, DateTime dateTime)
        {
            return new LogEntity
            {
                PartitionKey = GeneratePartitionKey(dateTime),
                DateTime = dateTime,
                Level = level,
                Component = component,
                Process = process,
                Context = context,
                Type = type,
                Stack = stack,
                Msg = msg
            };
        }

    }

    public class LykkeLogToAzureStorage : ProducerConsumer<LogEntity>, ILog
    {
        private readonly INoSQLTableStorage<LogEntity> _tableStorage;
        private ISlackNotificationsSender _slackNotificationsSender;

        public LykkeLogToAzureStorage(string applicationName, INoSQLTableStorage<LogEntity> tableStorage, 
            ISlackNotificationsSender slackNotificationsSender = null)
            :base(applicationName, null)
        {
            _tableStorage = tableStorage;
            _slackNotificationsSender = slackNotificationsSender;
        }

        public LykkeLogToAzureStorage SetSlackNotification(ISlackNotificationsSender slackNotificationsSender)
        {
            _slackNotificationsSender = slackNotificationsSender;
            return this;
        }


        private Task Insert(string level, string component, string process, string context, string type, string stack,
            string msg, DateTime? dateTime)
        {
            var dt = dateTime ?? DateTime.UtcNow;
            var newEntity = LogEntity.Create(level, component, process, context, type, stack, msg, dt);
            Produce(newEntity);
            return Task.FromResult(0);
        }

        private const string ErrorType = "error";
        private const string FatalErrorType = "fatalerror";

        private const string WarningType = "warning";
        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert("info", component, process, context, null, null, info, dateTime);
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert(WarningType, component, process, context, null, null, info, dateTime);
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            return Insert("error", component, process, context, type.GetType().ToString(), type.StackTrace, type.GetBaseException().Message, dateTime);
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            return Insert(FatalErrorType, component, process, context, type.GetType().ToString(), type.StackTrace, type.GetBaseException().Message, dateTime);
        }

        protected override async Task Consume(LogEntity item)
        {
            await _tableStorage.InsertAndGenerateRowKeyAsTimeAsync(item, item.DateTime);

            if (_slackNotificationsSender != null)
            {
                if (item.Level == ErrorType || item.Level == FatalErrorType)
                    await _slackNotificationsSender.SendErrorAsync(item.Component +" : " + item.Msg + " : " + item.Stack);

                if (item.Level == WarningType)
                    await _slackNotificationsSender.SendWarningAsync(item.Component + " : " + item.Msg);
            }

        }
    }

}

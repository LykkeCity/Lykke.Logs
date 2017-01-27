using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Log;
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

    public class LykkeLogSystem : ProducerConsumer<LogEntity>, ILog
    {
        private readonly INoSQLTableStorage<LogEntity> _tableStorage;

        public LykkeLogSystem(string applicationName, INoSQLTableStorage<LogEntity> tableStorage)
            :base(applicationName, null)
        {
            _tableStorage = tableStorage;
        }

        private Task Insert(string level, string component, string process, string context, string type, string stack,
            string msg, DateTime? dateTime)
        {
            var dt = dateTime ?? DateTime.UtcNow;
            var newEntity = LogEntity.Create(level, component, process, context, type, stack, msg, dt);
            Produce(newEntity);
            return Task.FromResult(0);
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert("info", component, process, context, null, null, info, dateTime);
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert("warning", component, process, context, null, null, info, dateTime);
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            return Insert("error", component, process, context, type.GetType().ToString(), type.StackTrace, type.Message, dateTime);
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            return Insert("fatalerror", component, process, context, type.GetType().ToString(), type.StackTrace, type.Message, dateTime);
        }

        protected override async Task Consume(LogEntity item)
        {
            await _tableStorage.InsertAndGenerateRowKeyAsTimeAsync(item, item.DateTime);
        }
    }

}

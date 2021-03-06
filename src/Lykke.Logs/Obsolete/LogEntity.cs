using System;
using JetBrains.Annotations;
using Lykke.Common;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs
{
    [Obsolete("Use new Lykke logging system")]
    public class LogEntity : TableEntity
    {
        [UsedImplicitly]
        public DateTime DateTime { get; set; }
        [UsedImplicitly]
        public string Level { get; set; }
        [UsedImplicitly]
        public string Env { get; set; }
        [UsedImplicitly]
        public string AppName { get; set; }
        [UsedImplicitly]
        public string Version { get; set; }
        [UsedImplicitly]
        public string Component { get; set; }
        [UsedImplicitly]
        public string Process { get; set; }
        [UsedImplicitly]
        public string Context { get; set; }
        [UsedImplicitly]
        public string Type { get; set; }
        [UsedImplicitly]
        public string Stack { get; set; }
        [UsedImplicitly]
        public string Msg { get; set; }

        public static LogEntity CreateWithoutRowKey(
            string level,
            string component,
            string process,
            string context,
            string type,
            string stack,
            string msg,
            DateTime dateTime)
        {
            return new LogEntity
            {
                PartitionKey = GeneratePartitionKey(dateTime),
                DateTime = dateTime,
                Level = level,
                Env = AppEnvironment.EnvInfo,
                AppName = AppEnvironment.Name,
                Version = AppEnvironment.Version,
                Component = component,
                Process = process,
                Context = Truncate(context),
                Type = type,
                Stack = Truncate(stack),
                Msg = Truncate(msg)
            };
        }

        private static string GeneratePartitionKey(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static string GenerateRowKey(DateTime dateTime, int itemNumber, int retryNumber)
        {
            return retryNumber == 0
                ? $"{dateTime:HH:mm:ss.fffffff}.{itemNumber:000}"
                : $"{dateTime:HH:mm:ss.fffffff}.{itemNumber:000}.{retryNumber:000}";
        }

        private static string Truncate(string str)
        {
            if (str == null)
            {
                return null;
            }

            // See: https://blogs.msdn.microsoft.com/avkashchauhan/2011/11/30/how-the-size-of-an-entity-is-caclulated-in-windows-azure-table-storage/
            // String � # of Characters * 2 bytes + 4 bytes for length of string
            // Max coumn size is 64 Kb, so max string len is (65536 - 4) / 2 = 32766
            // 3 - is for "..."
            const int maxLength = 32766 - 3;

            if (str.Length > maxLength)
            {
                return string.Concat(str.Substring(0, maxLength), "...");
            }

            return str;
        }
    }
}
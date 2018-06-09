using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    internal sealed class LogEntity : TableEntity
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
            [NotNull] string appName,
            [NotNull] string appVersion,
            [NotNull] string envInfo,
            [NotNull] string level,
            [NotNull] string component,
            [NotNull] string process,
            [CanBeNull] string context,
            [CanBeNull] string type,
            [CanBeNull] string stack,
            [CanBeNull] string message,
            DateTime dateTime)
        {
            return new LogEntity
            {
                PartitionKey = GeneratePartitionKey(dateTime),
                DateTime = dateTime,
                Level = level ?? throw new ArgumentNullException(nameof(level)),
                Env = envInfo ?? throw new ArgumentNullException(nameof(envInfo)),
                AppName = appName ?? throw new ArgumentNullException(nameof(appName)),
                Version = appVersion ?? throw new ArgumentNullException(nameof(appVersion)),
                Component = component ?? throw new ArgumentNullException(nameof(component)),
                Process = process ?? throw new ArgumentNullException(nameof(process)),
                Context = Truncate(context),
                Type = type,
                Stack = Truncate(stack),
                Msg = Truncate(message)
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
            // String – # of Characters * 2 bytes + 4 bytes for length of string
            // Max coumn size is 64 Kb, so max string len is (65536 - 4) / 2 = 32766
            // 3 - is for "..."
            const int maxLength = 32766 - 3;

            if (str.Length > maxLength)
            {
                var builder = new StringBuilder();

                builder.Append(str, 0, maxLength);
                builder.Append("...");

                return builder.ToString();
            }

            return str;
        }
    }
}
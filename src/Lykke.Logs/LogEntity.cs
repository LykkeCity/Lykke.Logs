﻿using System;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Logs
{
    public class LogEntity : TableEntity
    {
        public DateTime DateTime { get; set; }
        public string Level { get; set; }
        public string Version { get; set; }
        public string Component { get; set; }
        public string Process { get; set; }
        public string Context { get; set; }
        public string Type { get; set; }
        public string Stack { get; set; }
        public string Msg { get; set; }

        private static readonly string AppVersion;

        static LogEntity()
        {
            AppVersion = PlatformServices.Default.Application.ApplicationVersion;
        }
        
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
                Version = AppVersion,
                Component = component,
                Process = process,
                Context = context,
                Type = type,
                Stack = stack,
                Msg = msg
            };
        }

        public static string GeneratePartitionKey(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static string GenerateRowKey(DateTime dateTime, int itemNumber, int retryNumber)
        {
            return retryNumber == 0 
                ? $"{dateTime:HH:mm:ss.fffffff}.{itemNumber:000}" 
                : $"{dateTime:HH:mm:ss.fffffff}.{itemNumber:000}.{retryNumber:000}";
        }
    }
}
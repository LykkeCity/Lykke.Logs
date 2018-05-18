using System;
using System.Threading;
using Lykke.Common.Log;
using Lykke.Logs;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ConsoleLoggerRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new LykkeConsoleLoggerProvider((s, level) => true, false);
            var logger = provider.CreateLogger("ComponentName");

            for (int i = 0; i < 1000; i++)
            {
                logger.Log(LogLevel.Warning, new EventId(0), GetState(), null, (parameters, exception) => parameters.Message);
                logger.Log(LogLevel.Critical, new EventId(0), GetState(), new OutOfMemoryException("Good buy"), (parameters, exception) => parameters.Message);
            }

            logger.Log(LogLevel.Trace, new EventId(0), GetState(), null, (parameters, exception) => parameters.Message);
            logger.Log(LogLevel.Debug, new EventId(0), GetState(), null, (parameters, exception) => parameters.Message);
            logger.Log(LogLevel.Information, new EventId(0), GetState(), null, (parameters, exception) => parameters.Message);
            logger.Log(LogLevel.Warning, new EventId(0), GetState(), null, (parameters, exception) => parameters.Message);


            logger.Log(LogLevel.Error, new EventId(0), GetState(), new InvalidOperationException("Something goes wrong"), (parameters, exception) => parameters.Message);
            logger.Log(LogLevel.Critical, new EventId(0), GetState(), new OutOfMemoryException("Good buy"), (parameters, exception) => parameters.Message);

            var scopedProvider = new LykkeConsoleLoggerProvider((s, level) => true, true);
            var scopedLogger = scopedProvider.CreateLogger("ScopedComponent");
            scopedLogger.Log(LogLevel.Information, new EventId(0), GetState(), null, (parameters, exception) => parameters.Message);

            using (scopedLogger.BeginScope("Hi I am a scope {0}", 1))
            {
                scopedLogger.Log(LogLevel.Information, new EventId(0), GetState(), null, (parameters, exception) => parameters.Message);
                scopedLogger.Log(LogLevel.Error, new EventId(0), GetState(), new InvalidOperationException("Something goes wrong"), (parameters, exception) => parameters.Message);
                scopedLogger.Log(LogLevel.Critical, new EventId(0), GetState(), new OutOfMemoryException("Good buy"), (parameters, exception) => parameters.Message);
            }


            Console.ReadLine();
        }

        private static LogEntryParameters GetState()
        {
            var state = new LogEntryParameters("AppName", "1.01", "Env", "Caller", "Process1", 12, "MyMessage", new {Prop1 = "SomeValue", Prop2 = "OtherValue"}, DateTime.Now);
            return state;
        }
    }
}

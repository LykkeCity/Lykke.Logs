using System;
using System.Threading;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console.Internal;
using NSubstitute;
using Xunit;

namespace Lykke.Logs.Tests
{
    public class ConsoleOutputTest : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConsole _console;
        private readonly LykkeConsoleLoggerProvider _provider;

        public ConsoleOutputTest()
        {
            _console = Substitute.For<IConsole>();
            
            _provider = new LykkeConsoleLoggerProvider(new ConsoleLoggerOptions(), new ConsoleLogMessageWriter(_console));

            _logger = _provider.CreateLogger("MyLogger");
        }

        [Fact]
        public void ShouldWriteFormattedOutput()
        {
            var state = new LogEntryParameters("AppName", "1.01", "Env", "Caller", "Process1", 12, "MyMessage", null, DateTime.Now);

            _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, new EventId(0), state, null, (parameters, exception) => parameters.Message);

            var expected = $": {state.Moment:MM-dd HH:mm:ss.fff} : MyLogger : {state.Process}{Environment.NewLine}      {state.Message}{Environment.NewLine}";
            
            Thread.Sleep(100);

            _console.Received(1).Write("INFO", null, ConsoleColor.Gray);
            _console.Received(1).Write(expected, null, null);
        }

        [Fact]
        public void ProviderShouldReturnCorrectLogger()
        {
            using (var provider = new LykkeConsoleLoggerProvider(new ConsoleLoggerOptions(), Substitute.For<IConsoleLogMessageWriter>()))
            {
                var logger = provider.CreateLogger("SupperLogger");

                Assert.IsType<LykkeConsoleLogger>(logger);
            }
        }

        public void Dispose()
        {
            _provider.Dispose();
        }
    }
}
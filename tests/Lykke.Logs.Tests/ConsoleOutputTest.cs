using System;
using System.Threading;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console.Internal;
using NSubstitute;
using Xunit;

namespace Lykke.Logs.Tests
{
    public class ConsoleOutputTest
    {
        private readonly LoggerFactory _loggerFactory;
        private readonly LykkeConsoleLogger _logger;
        private readonly IConsole _console;

        public ConsoleOutputTest()
        {
            _loggerFactory = new LoggerFactory(new[] { new LykkeConsoleLoggerProvider(Filter, false) });
            _console = Substitute.For<IConsole>();
            var logger = new LykkeConsoleLogger("MyLogger", (s, level) => true, false)
            {
                Console = _console
            };
            _logger = logger;
        }

        [Fact]
        public void ShouldWriteFormattedOutput()
        {
            var state = new LogEntryParameters("AppName", "1.01", "Env", "Caller", "Process1", 12, "MyMessage", null, DateTime.Now);
            _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, new EventId(0), state, null, (parameters, exception) => parameters.Message);

            var expected = $"{state.Moment:yyyy-MM-dd HH:mm:ss:fff} [{"INFO"}] {_logger.Name}:{state.Process}:{state.Context} - {state.Message}";
            Thread.Sleep(50);
            _console.Received().WriteLine(expected, null, ConsoleColor.Gray);
        }

        private static bool Filter(string arg1, Microsoft.Extensions.Logging.LogLevel arg2)
        {
            return true;
        }
    }
}
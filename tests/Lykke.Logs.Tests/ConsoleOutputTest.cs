using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console.Internal;
using Xunit;

namespace Lykke.Logs.Tests
{
    public class ConsoleOutputTest
    {
        private readonly LoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public ConsoleOutputTest()
        {
            _loggerFactory = new LoggerFactory(new[] { new LykkeConsoleLoggerProvider(Filter, false) });
            var logger = _loggerFactory.CreateLogger("TestConsoleLogger");
            _logger = logger;
        }

        [Fact]
        public void ShouldWriteFormattedOutput()
        {

        }

        private static bool Filter(string arg1, Microsoft.Extensions.Logging.LogLevel arg2)
        {
            return true;
        }

        private class TestConsole : IConsole
        {
            public string LastWrite;
            public string LastWriteLine;
            public bool LastFlush;

            public void Write(string message, ConsoleColor? background, ConsoleColor? foreground)
            {
                throw new NotImplementedException();
            }

            public void WriteLine(string message, ConsoleColor? background, ConsoleColor? foreground)
            {
                throw new NotImplementedException();
            }

            public void Flush()
            {
                throw new NotImplementedException();
            }
        }
    }
}
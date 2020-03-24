using System;
using System.Threading;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal class ConsoleLogMessageWriter : IConsoleLogMessageWriter
    {
        public static ConsoleLogMessageWriter Instance { get; } = new ConsoleLogMessageWriter(ConsoleProvider.Console);

        private readonly IConsole _console;

        private readonly SemaphoreSlim _lock;

        public ConsoleLogMessageWriter(IConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));

            _lock = new SemaphoreSlim(1, 1);
        }

        public void Write(LogMessageEntry entry)
        {
            _lock.Wait();

            try
            {
                if (entry.LevelString != null)
                {
                    _console.Write(entry.LevelString, entry.LevelBackground, entry.LevelForeground);
                }
                _console.Write(entry.Message, entry.MessageColor, entry.MessageColor);
            }
            finally
            {
                _lock.Release();
            }

            _console.Flush();
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}
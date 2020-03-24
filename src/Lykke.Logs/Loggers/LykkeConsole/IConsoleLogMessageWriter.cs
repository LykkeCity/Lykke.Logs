using System;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal interface IConsoleLogMessageWriter : IDisposable
    {
        void Write(LogMessageEntry entry);
    }
}
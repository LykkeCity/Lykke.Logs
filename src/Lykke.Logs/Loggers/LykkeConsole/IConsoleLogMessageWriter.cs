using System;
using Microsoft.Extensions.Logging.Console.Internal;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal interface IConsoleLogMessageWriter : IDisposable
    {
        void Write(LogMessageEntry entry);
    }
}
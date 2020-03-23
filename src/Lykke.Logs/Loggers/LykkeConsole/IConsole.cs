using System;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal interface IConsole
    {
        void Write(string message, ConsoleColor? background, ConsoleColor? foreground);

        void WriteLine(string message, ConsoleColor? background, ConsoleColor? foreground);

        void Flush();
    }
}

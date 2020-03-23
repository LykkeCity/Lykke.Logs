using System;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal struct LogMessageEntry
    {
        public string LevelString;
        public ConsoleColor? LevelBackground;
        public ConsoleColor? LevelForeground;
        public ConsoleColor? MessageColor;
        public string Message;
    }
}

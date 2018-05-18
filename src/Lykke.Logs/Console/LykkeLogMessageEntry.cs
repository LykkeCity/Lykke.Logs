using System;

namespace  Lykke.Logs
{
    internal struct LykkeLogMessageEntry
    {
        public string LevelString;
        public ConsoleColor? MessageBackground;
        public ConsoleColor? MessageForeground;
        public string Message;
    }
}

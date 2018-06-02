using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Logs
{
    internal static class LogLevels
    {
        public static IReadOnlyCollection<Microsoft.Extensions.Logging.LogLevel> All { get; }

        static LogLevels()
        {
            All = Enum.GetValues(typeof(Microsoft.Extensions.Logging.LogLevel))
                .Cast<Microsoft.Extensions.Logging.LogLevel>()
                .Where(l => l != Microsoft.Extensions.Logging.LogLevel.None)
                .ToArray();
        }
    }
}
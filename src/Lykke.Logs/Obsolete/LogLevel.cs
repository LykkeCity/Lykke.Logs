using System;

namespace Lykke.Logs
{
    /// <summary>
    /// Specifies log level
    /// </summary>
    [Obsolete("Use new Lykke logging system and Microsoft.Extensions.Logging.LogLevel")]
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Info = 1,
        Warning = 1 << 1,
        Error = 1 << 2,
        FatalError = 1 << 3,
        Monitoring = 1 << 4,
        All = Info | Warning | Error | FatalError | Monitoring
    }
}
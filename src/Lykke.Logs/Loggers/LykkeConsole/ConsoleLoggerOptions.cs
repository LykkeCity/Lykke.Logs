using Common.Log;
using JetBrains.Annotations;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    /// <summary>
    /// Additional options for the Console logger
    /// </summary>
    [PublicAPI]
    public class ConsoleLoggerOptions
    {
        /// <summary>
        /// Includes scopes information in the messages. See <see cref="ILog.BeginScope"/>.
        /// Default is true
        /// </summary>
        public bool IncludeScopes { get; set; }

        internal ConsoleLoggerOptions()
        {
            IncludeScopes = true;
        }
    }
}
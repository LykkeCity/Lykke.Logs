using JetBrains.Annotations;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    /// <summary>
    /// Additional options for the additional Slack logger
    /// </summary>
    [PublicAPI]
    public class AdditionalSlackLoggerOptions : SlackLoggerOptions
    {
        /// <summary>
        /// Minimal logging level. Default is <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/>
        /// </summary>
        public Microsoft.Extensions.Logging.LogLevel MinLogLevel { get; set; }

        internal AdditionalSlackLoggerOptions([NotNull] ISpamGuardConfiguration<Microsoft.Extensions.Logging.LogLevel> spamGuard) : 
            base(spamGuard)
        {
            MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Information;
        }
    }
}
using JetBrains.Annotations;
using Lykke.Common.Log;

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

        /// <summary>
        /// Flag that toggles adding messages from <see cref="IHealthNotifier " /> to custom slack channel.
        /// </summary>
        public bool AreHealthNotificationsIncluded { get; private set; }

        public void IncludeHealthNotifications()
        {
            AreHealthNotificationsIncluded = true;
        }

        internal AdditionalSlackLoggerOptions([NotNull] ISpamGuardConfiguration<Microsoft.Extensions.Logging.LogLevel> spamGuard) : 
            base(spamGuard)
        {
            MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Information;
        }
    }
}
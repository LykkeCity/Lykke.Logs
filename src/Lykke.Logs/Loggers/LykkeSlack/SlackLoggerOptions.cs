using System;
using JetBrains.Annotations;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    /// <summary>
    /// Additional options for the essential Slack logger
    /// </summary>
    [PublicAPI]
    public class SlackLoggerOptions
    {
        internal bool IsChaosExceptionFilteringEnabled { get; private set; }

        /// <summary>Spam guard configuration</summary>
        public ISpamGuardConfiguration<Microsoft.Extensions.Logging.LogLevel> SpamGuard { get; }

        public SlackLoggerOptions()
        {
            IsChaosExceptionFilteringEnabled = true;
        }

        public void DisableChaosExceptionFiltering()
        {
            IsChaosExceptionFilteringEnabled = false;
        }

        internal SlackLoggerOptions([NotNull] ISpamGuardConfiguration<Microsoft.Extensions.Logging.LogLevel> spamGuard)
        {
            SpamGuard = spamGuard ?? throw new ArgumentNullException(nameof(spamGuard));
        }
    }
}
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
        internal bool FilterOutChaosException { get; private set; }

        /// <summary>Spam guard configuration</summary>
        public ISpamGuardConfiguration<Microsoft.Extensions.Logging.LogLevel> SpamGuard { get; }

        public SlackLoggerOptions()
        {
            FilterOutChaosException = true;
        }

        public void DisableChaosExceptionFiltering()
        {
            FilterOutChaosException = false;
        }

        internal SlackLoggerOptions([NotNull] ISpamGuardConfiguration<Microsoft.Extensions.Logging.LogLevel> spamGuard)
        {
            SpamGuard = spamGuard ?? throw new ArgumentNullException(nameof(spamGuard));
        }
    }
}
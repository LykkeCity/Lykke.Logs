using System;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    internal static class SlackSpamGuardBuilder
    {
        public static SpamGuard<Microsoft.Extensions.Logging.LogLevel> BuildForEssentialSlackChannelsSpamGuard()
        {
            var spamGuard = new SpamGuard<Microsoft.Extensions.Logging.LogLevel>(LogFactory.LastResort);

            foreach (var level in new[]
            {
                Microsoft.Extensions.Logging.LogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error
            })
            {
                spamGuard.SetMutePeriod(level, TimeSpan.FromMinutes(1));
            }

            spamGuard.Start();

            return spamGuard;
        }

        public static SpamGuard<Microsoft.Extensions.Logging.LogLevel> BuildForAdditionalSlackChannel()
        {
            var spamGuard = new SpamGuard<Microsoft.Extensions.Logging.LogLevel>(LogFactory.LastResort);

            foreach (var level in new[]
            {
                Microsoft.Extensions.Logging.LogLevel.Information,
                Microsoft.Extensions.Logging.LogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error
            })
            {
                spamGuard.SetMutePeriod(level, TimeSpan.FromMinutes(1));
            }

            spamGuard.Start();

            return spamGuard;
        }
    }
}
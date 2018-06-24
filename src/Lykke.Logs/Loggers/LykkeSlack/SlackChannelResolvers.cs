using System;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    internal static class SlackChannelResolvers
    {
        public static Func<Microsoft.Extensions.Logging.LogLevel, string> EssentialChannelsResolver { get; } =
            level =>
            {
                switch (level)
                {
                    case Microsoft.Extensions.Logging.LogLevel.Trace:
                    case Microsoft.Extensions.Logging.LogLevel.Debug:
                    case Microsoft.Extensions.Logging.LogLevel.Information:
                        return null;

                    case Microsoft.Extensions.Logging.LogLevel.Warning:
                        return "Warning";
                    case Microsoft.Extensions.Logging.LogLevel.Error:
                    case Microsoft.Extensions.Logging.LogLevel.Critical:
                        return "Errors";

                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            };

        public static Func<Microsoft.Extensions.Logging.LogLevel, string> GetAdditionalChannelResolver(
            Microsoft.Extensions.Logging.LogLevel minLogLevel, 
            string channel)
        {
            return level => level >= minLogLevel ? channel : null;
        }
    }
}
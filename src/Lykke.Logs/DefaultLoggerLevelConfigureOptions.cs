using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Logs
{
    internal class DefaultLoggerLevelConfigureOptions : ConfigureOptions<LoggerFilterOptions>
    {
        public DefaultLoggerLevelConfigureOptions(Microsoft.Extensions.Logging.LogLevel level) : 
            base(options => options.MinLevel = level)
        {
        }
    }
}
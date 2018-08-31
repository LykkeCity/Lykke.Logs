using System.Text.RegularExpressions;
using Common.Log;

namespace Lykke.Logs.Loggers.LykkeSanitizing
{
    public static class LogExtensions
    {
        /// <summary>
        /// Wraps log into sanitizing decorator, which is able to remove sensitive data like keys, passwords, etc., while logging.
        /// <see cref="ISanitizingLog"/> instance itself does nothing, you should call <see cref="ISanitizingLog.AddSanitizingFilter(Regex, string)"/> to add sensitive data filter(s).
        /// </summary>
        /// <param name="log">Original log instance.</param>
        /// <returns>New <see cref="ISanitizingLog"/> instance.</returns>
        public static ISanitizingLog Sanitize(this ILog log)
        {
            return new SanitizingLog(log);
        }
    }
}
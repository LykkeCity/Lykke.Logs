using System.Text.RegularExpressions;
using Common.Log;

namespace Lykke.Logs.Loggers.LykkeSanitizing
{
    /// <summary>
    /// <see cref="ILog" /> decorator for sanitizing log data (removing sensitive data like keys, passwords, etc.).
    /// </summary>
    public interface ISanitizingLog : ILog
    {
        /// <summary>
        /// Adds sensitive pattern that should not be logged. Api keys, private keys and so on.
        /// </summary>
        /// <param name="pattern">Regex to recognize data that should be replaced.</param>
        /// <param name="replacement">String to insert, can be empty string.</param>
        /// <returns>Self instance to continue adding filters.</returns>
        ISanitizingLog AddSanitizingFilter(Regex pattern, string replacement);
    }
}
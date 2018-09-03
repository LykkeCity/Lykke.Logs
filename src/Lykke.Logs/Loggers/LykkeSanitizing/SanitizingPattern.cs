using System.Text.RegularExpressions;

namespace Lykke.Logs.Loggers.LykkeSanitizing
{
    internal sealed class SanitizingFilter
    {
        public SanitizingFilter(Regex pattern, string replacement)
        {
            Pattern = pattern ?? throw new System.ArgumentNullException(nameof(pattern));
            Replacement = replacement ?? throw new System.ArgumentNullException(nameof(replacement));
        }

        public Regex Pattern { get; }
        public string Replacement { get; }
    }
}
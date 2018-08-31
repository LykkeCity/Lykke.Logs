using System.Collections.Generic;

namespace Lykke.Logs.Loggers.LykkeSanitizing
{
    internal sealed class SanitizingOptions
    {
        public ICollection<SanitizingFilter> Filters { get; } = new List<SanitizingFilter>();
    }
}
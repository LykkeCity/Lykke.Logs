using System;

namespace Lykke.Logs.Loggers.LykkeSanitizing
{
    internal sealed class SanitizingException : Exception
    {
        private readonly Exception _exception;
        private readonly Func<string, string> _sanitizer;

        public SanitizingException(Exception exception, Func<string, string> sanitizer)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
            _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
        }

        public override string Message => _sanitizer(_exception.Message);
        public override string Source => _sanitizer(_exception.Source);
        public override string ToString() => _sanitizer(_exception.ToString());
        public override string StackTrace => _exception.StackTrace;
    }
}
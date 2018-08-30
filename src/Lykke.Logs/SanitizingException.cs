using System;

namespace Lykke.Logs
{
    /// <summary>
    /// <see cref="Exception" /> decorator for sanitizing exception data (removing sensitive data like keys, passwords, etc.).
    /// </summary>
    public class SanitizingException : Exception
    {
        private readonly Exception _exception;
        private readonly Func<string, string> _sanitizer;

        /// <summary>
        /// Initializes class instance.
        /// </summary>
        /// <param name="exception">Original exception.</param>
        /// <param name="sanitizer">Function to sanitize exception data.</param>
        public SanitizingException(Exception exception, Func<string, string> sanitizer)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
            _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
        }

        public override string Message => _sanitizer(_exception.Message);
        public override string Source => _sanitizer(_exception.Source);
        public override string ToString() => _sanitizer(_exception.ToString());
    }
}
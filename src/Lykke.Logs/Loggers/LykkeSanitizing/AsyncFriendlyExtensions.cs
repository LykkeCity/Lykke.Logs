using System;

namespace Lykke.Logs.Loggers.LykkeSanitizing
{
    internal static class AsyncFriendlyExtensions
    {
        private const string AsyncStackTraceExceptionData = "AsyncFriendlyStackTrace";
        
        public static SanitizingException AddAsyncFriendlyStackTrace(this SanitizingException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            
            if (!exception.Data.Contains(AsyncStackTraceExceptionData))
                exception.Data.Add(AsyncStackTraceExceptionData, exception.StackTrace);

            return exception;
        }
    }
}
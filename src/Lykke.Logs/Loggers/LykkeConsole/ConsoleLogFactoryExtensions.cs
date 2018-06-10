using System;
using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    [PublicAPI]
    public static class ConsoleLogFactoryExtensions
    {
        /// <summary>
        /// Adds a buffered console logger.
        /// This is common console logger.
        /// </summary>
        /// <param name="factory">The <see cref="ILogFactory"/> to use.</param>
        /// <param name="configure">Optional configuration</param>
        public static ILogFactory AddConsole(
            [NotNull] this ILogFactory factory,
            Action<ConsoleLoggerOptions> configure = null)
        {
            var options = new ConsoleLoggerOptions();

            configure?.Invoke(options);

            return factory.AddConsole(options, new BufferedConsoleLogMessageWriterDecorator(ConsoleLogMessageWriter.Instance));
        }

        /// <summary>
        /// Adds an unbuffered console logger.
        /// Useful for tests.
        /// </summary>
        /// <param name="factory">The <see cref="ILogFactory"/> to use.</param>
        /// <param name="configure">Optional configuration</param>
        public static ILogFactory AddUnbufferedConsole(
            [NotNull] this ILogFactory factory,
            Action<ConsoleLoggerOptions> configure = null)
        {
            var options = new ConsoleLoggerOptions();
            
            configure?.Invoke(options);

            return factory.AddConsole(options, ConsoleLogMessageWriter.Instance);
        }

        private static ILogFactory AddConsole(
            [NotNull] this ILogFactory factory,
            [NotNull] ConsoleLoggerOptions options,
            [NotNull] IConsoleLogMessageWriter messageWriter)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new LykkeConsoleLoggerProvider(options, messageWriter));

            return factory;
        }
    }
}
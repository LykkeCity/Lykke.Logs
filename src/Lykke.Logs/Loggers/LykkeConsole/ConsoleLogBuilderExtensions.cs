using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal static class ConsoleLogBuilderExtensions
    {
        public static ILogBuilder AddConsole(
            [NotNull] this ILogBuilder builder,
            [CanBeNull] Action<ConsoleLoggerOptions> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new ConsoleLoggerOptions();

            configure?.Invoke(options);

            builder.Services.AddSingleton<ILoggerProvider, LykkeConsoleLoggerProvider>(s =>
                new LykkeConsoleLoggerProvider(
                    options,
                    new BufferedConsoleLogMessageWriterDecorator(ConsoleLogMessageWriter.Instance)));

            return builder;
        }
    }
}
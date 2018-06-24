using System;
using System.Text;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using Microsoft.Extensions.Logging.Console.Internal;

namespace Lykke.Logs
{
    /// <summary>
    /// Health notifier, that notifies to the console. Could be used in tests
    /// </summary>
    [PublicAPI]
    public class ConsoleHealthNotifier : IHealthNotifier
    {
        public static IHealthNotifier Instance { get; } = new ConsoleHealthNotifier(ConsoleLogMessageWriter.Instance);

        private readonly IConsoleLogMessageWriter _messageWriter;

        private ConsoleHealthNotifier([NotNull] IConsoleLogMessageWriter messageWriter)
        {
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
        }

        public void Notify(string healthMessage, object context = null)
        {
            var messageBuilder = new StringBuilder();

            messageBuilder.Append(healthMessage);

            if (context != null)
            {
                messageBuilder.AppendLine();
                messageBuilder.Append(LogContextConversion.ConvertToString(context));
            }

            _messageWriter.Write(new LogMessageEntry
            {
                LevelForeground = ConsoleColor.White,
                LevelString = "[HEALTH]",
                Message = messageBuilder.ToString()
            });
        }

        public void Dispose()
        {
        }
    }
}
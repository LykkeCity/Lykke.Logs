using System;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal class WindowsLogConsole : IConsole
    {
        private void SetColor(ConsoleColor? background, ConsoleColor? foreground)
        {
            if (background.HasValue)
                Console.BackgroundColor = background.Value;

            if (foreground.HasValue)
                Console.ForegroundColor = foreground.Value;
        }

        private void ResetColor()
        {
            Console.ResetColor();
        }

        public void Write(string message, ConsoleColor? background, ConsoleColor? foreground)
        {
            SetColor(background, foreground);
            Console.Out.Write(message);
            ResetColor();
        }

        public void WriteLine(string message, ConsoleColor? background, ConsoleColor? foreground)
        {
            SetColor(background, foreground);
            Console.Out.WriteLine(message);
            ResetColor();
        }

        public void Flush()
        {
            // No action required as for every write, data is sent directly to the console
            // output stream
        }
    }
}

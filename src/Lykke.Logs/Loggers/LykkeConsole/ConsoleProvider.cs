using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging.Console.Internal;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal static class ConsoleProvider
    {
        private class AnsiSystemConsole : IAnsiSystemConsole
        {
            public void Write(string message)
            {
                System.Console.Write(message);
            }

            public void WriteLine(string message)
            {
                System.Console.WriteLine(message);
            }
        }

        public static IConsole Console => ConsoleInitializer.Value;

        private static readonly Lazy<IConsole> ConsoleInitializer;

        static ConsoleProvider()
        {
            ConsoleInitializer = new Lazy<IConsole>(
                () =>
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return new WindowsLogConsole();
                    }

                    return new AnsiLogConsole(new AnsiSystemConsole());
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
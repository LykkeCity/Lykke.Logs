using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Lykke.Logs.Loggers.LykkeConsole
{
    internal static class ConsoleProvider
    {
        public static IConsole Console => ConsoleInitializer.Value;

        private static readonly Lazy<IConsole> ConsoleInitializer;

        static ConsoleProvider()
        {
            ConsoleInitializer = new Lazy<IConsole>(
                () =>
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        return new WindowsLogConsole();

                    return new AnsiLogConsole();
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
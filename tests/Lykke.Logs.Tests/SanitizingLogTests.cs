using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using NSubstitute;
using Xunit;
using IConsole = Microsoft.Extensions.Logging.Console.Internal.IConsole;
using Level = Microsoft.Extensions.Logging.LogLevel;

namespace Lykke.Logs.Tests
{
    public class SanitizingLogTests
    {
        private IConsole _console;
        private ILog _log;

        public SanitizingLogTests()
        {
            _console = Substitute.For<IConsole>();
            _log = new SanitizingLog(new Log(new LykkeConsoleLoggerProvider(new ConsoleLoggerOptions(), new ConsoleLogMessageWriter(_console)).CreateLogger("Test"), Substitute.For<IHealthNotifier>()))
                .AddSensitivePattern(new Regex(@"""privateKey"": ""(.*)"""), "\"privateKey\": \"*\"");
        }

        [Fact]
        public void ShouldSanitizeLog()
        {
            // Arrange

            var secret = "qwertyuiop";
            var patternedString = $"\"privateKey\": \"{secret}\"";
            var patternedObject = new { privateKey = secret };
            var patternedException = new Exception(patternedString);

            // Act
            // Get and call all available logging methods

            var extMethods = typeof(MicrosoftLoggingBasedLogExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var logMethods = typeof(ILog).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name.StartsWith("Write"));

            foreach (var m in extMethods.Concat(logMethods))
            {
                var args = m.GetParameters()
                    .Select(p =>
                        p.ParameterType == typeof(string) ? patternedString :
                        p.ParameterType == typeof(object) ? patternedObject :
                        p.ParameterType == typeof(Exception) ? (object)patternedException :
                        p.ParameterType == typeof(ILog) ? (object)_log :
                        p.ParameterType == typeof(Level) ? (object)Level.Information :
                        p.ParameterType == typeof(int) ? (object)1 :
                        null)
                    .ToArray();

                var task = m.Invoke(m.IsStatic ? null : _log, args) as Task;
                if (task != null)
                {
                    task.Wait();
                }
            }

            // Assert

            var writeMethodCalls = _console.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name.StartsWith("Write"));

            Assert.NotEmpty(writeMethodCalls);
            Assert.DoesNotContain(writeMethodCalls, 
                c => c.GetArguments().OfType<string>().Any(a => a.Contains(secret)));
        }
    }
}
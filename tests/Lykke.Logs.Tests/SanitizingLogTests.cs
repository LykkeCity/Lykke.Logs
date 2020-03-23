using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Logs.Loggers.LykkeSanitizing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using IConsole = Lykke.Logs.Loggers.LykkeConsole.IConsole;
using Level = Microsoft.Extensions.Logging.LogLevel;

namespace Lykke.Logs.Tests
{
    public class SanitizingLogTests
    {
        private IConsole _console;
        private ISanitizingLog _log;

        public SanitizingLogTests()
        {
            _console = Substitute.For<IConsole>();
            _log = new Log(new LykkeConsoleLoggerProvider(new ConsoleLoggerOptions(), new ConsoleLogMessageWriter(_console)).CreateLogger("Test"), Substitute.For<IHealthNotifier>())
                .Sanitize();
        }

        [Fact]
        public void ShouldSanitizeLog()
        {
            // Arrange

            _log.AddSanitizingFilter(new Regex(@"""privateKey"": ""(.*)"""), "\"privateKey\": \"*\"");

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

        [Fact]
        public void ShouldSanitizeAllPatterns()
        {
            // Arrange

            _log
                .AddSanitizingFilter(new Regex(@"""privateKey"": ""(.*)"""), "\"privateKey\": \"*\"")
                .AddSanitizingFilter(new Regex(@"""password"": ""(.*)"""), "\"password\": \"*\"");

            var secret = "qwertyuiop";
            var patternedString = $"\"privateKey\": \"{secret}\", \"password\": \"{secret}\"";
            var patternedObject = new { privateKey = secret, password = secret };

            // Act

            _log.Info(patternedString, patternedObject);

            // Assert

            var writeMethodCalls = _console.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name.StartsWith("Write"));

            Assert.NotEmpty(writeMethodCalls);
            Assert.DoesNotContain(writeMethodCalls,
                c => c.GetArguments().OfType<string>().Any(a => a.Contains(secret)));
        }

        [Fact]
        public void ShouldConfigureOptions()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();

            // Act

            serviceCollection.Configure<SanitizingOptions>(x => x.Filters.Add(new SanitizingFilter(new Regex(""), "*")));
            serviceCollection.Configure<SanitizingOptions>(x => x.Filters.Add(new SanitizingFilter(new Regex(""), "#")));

            // Assert

            var options = serviceCollection.BuildServiceProvider().GetService<IOptions<SanitizingOptions>>();

            Assert.NotNull(options.Value);
            Assert.Equal(2, options.Value.Filters.Count);
            Assert.Contains(options.Value.Filters, f => f.Replacement == "*");
            Assert.Contains(options.Value.Filters, f => f.Replacement == "#");
        }

        [Fact]
        public void Sanitize_ValueIsNullWithoutFilters_NotThrowException()
        {
            // Arrange
            var fakeLog = Substitute.For<ILog>();
            var sanitizer = new SanitizingLog(fakeLog, new SanitizingOptions());

            // Act
            var result = sanitizer.Sanitize(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Sanitize_ValueIsNullWithFilters_NotThrowException()
        {
            // Arrange
            var fakeLog = Substitute.For<ILog>();
            var sanitizer = new SanitizingLog(fakeLog, new SanitizingOptions());
            sanitizer.AddSanitizingFilter(new Regex(""), "");

            // Act
            var result = sanitizer.Sanitize(null);

            // Assert
            Assert.Null(result);
        }
    }
}
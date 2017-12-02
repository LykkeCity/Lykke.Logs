using System;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.SlackNotifications;

namespace Lykke.Logs.Slack
{
    [PublicAPI]
    public sealed class LykkeLogToSlack : ILog
    {
        private readonly ISlackNotificationsSender _sender;
        private readonly string _channel;
        private readonly LogLevel _logLevel;

        private LykkeLogToSlack(ISlackNotificationsSender sender, string channel, LogLevel logLevel)
        {
            _sender = sender;
            _channel = channel;
            _logLevel = logLevel;
        }

        public static ILog Create(ISlackNotificationsSender sender, string channel, LogLevel logLevel = LogLevel.All)
        {
            return new LykkeLogToSlack(sender, channel, logLevel);
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            if (_logLevel.HasFlag(LogLevel.Info))
            {
                return _sender.SendAsync(_channel, ":information_source:", $"{GetComponentName(component)} : {process} : {info} : {context}");
            }

            return Task.CompletedTask;
        }

        private string GetComponentName(string component)
        {
            var sb = new StringBuilder();

            sb.Append($"{AppEnvironment.Name} {AppEnvironment.Version}");

            if (!string.IsNullOrWhiteSpace(AppEnvironment.EnvInfo))
            {
                sb.Append($" : {AppEnvironment.EnvInfo}");
            }

            if (AppEnvironment.Name == null || !AppEnvironment.Name.StartsWith(component))
            {
                sb.Append($" : {component}");
            }

            return sb.ToString();
        }

        public Task WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            if (_logLevel.HasFlag(LogLevel.Monitoring))
            {
                return _sender.SendAsync(_channel, ":loudspeaker:", $"{component} : {process} : {info} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            if (_logLevel.HasFlag(LogLevel.Warning))
            {
                return _sender.SendAsync(_channel, ":warning:", $"{component} : {process} : {info} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            if (_logLevel.HasFlag(LogLevel.Error))
            {
                return _sender.SendAsync(_channel, ":exclamation:", $"{component} : {process} : {exception} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            if (_logLevel.HasFlag(LogLevel.FatalError))
            {
                return _sender.SendAsync(_channel, ":no_entry:", $"{component} : {process} : {exception} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteInfoAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return WriteInfoAsync(AppEnvironment.Name, process, context, info, dateTime);
        }

        public Task WriteMonitorAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return WriteMonitorAsync(AppEnvironment.Name, process, context, info, dateTime);
        }

        public Task WriteWarningAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return WriteWarningAsync(AppEnvironment.Name, process, context, info, dateTime);
        }

        public Task WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            return WriteErrorAsync(AppEnvironment.Name, process, context, exception, dateTime);
        }

        public Task WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            return WriteFatalErrorAsync(AppEnvironment.Name, process, context, exception, dateTime);
        }
    }
}
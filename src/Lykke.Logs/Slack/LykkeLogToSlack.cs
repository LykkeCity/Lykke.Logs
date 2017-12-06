using System;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.SlackNotifications;

namespace Lykke.Logs.Slack
{
    /// <summary>
    /// Logs entries to the specified Slack channel. Which types of entries should be logged, can be configured
    /// </summary>
    [PublicAPI]
    public sealed class LykkeLogToSlack : ILog
    {
        private readonly ISlackNotificationsSender _sender;
        private readonly string _channel;
        private readonly bool _isInfoEnabled;
        private readonly bool _isMonitorEnabled;
        private readonly bool _isWarningEnabled;
        private readonly bool _isErrorEnabled;
        private readonly bool _isFatalErrorEnabled;
        private readonly string _componentNamePrefix;

        private LykkeLogToSlack(ISlackNotificationsSender sender, string channel, LogLevel logLevel)
        {
            _sender = sender;
            _channel = channel;

            _isInfoEnabled = logLevel.HasFlag(LogLevel.Info);
            _isMonitorEnabled = logLevel.HasFlag(LogLevel.Monitoring);
            _isWarningEnabled = logLevel.HasFlag(LogLevel.Warning);
            _isErrorEnabled = logLevel.HasFlag(LogLevel.Error);
            _isFatalErrorEnabled = logLevel.HasFlag(LogLevel.FatalError);

            _componentNamePrefix = GetComponentNamePrefix();
        }

        /// <summary>
        /// Creates logger with, which logs entries of the given <paramref name="logLevel"/>,
        /// to the given <paramref name="channel"/>, using given <paramref name="sender"/>
        /// </summary>
        public static ILog Create(ISlackNotificationsSender sender, string channel, LogLevel logLevel = LogLevel.All)
        {
            return new LykkeLogToSlack(sender, channel, logLevel);
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isInfoEnabled)
            {
                return _sender.SendAsync(_channel, ":information_source:", $"{GetComponentName(component)} : {process} : {info} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isMonitorEnabled)
            {
                return _sender.SendAsync(_channel, ":loudspeaker:", $"{GetComponentName(component)} : {process} : {info} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isWarningEnabled)
            {
                return _sender.SendAsync(_channel, ":warning:", $"{GetComponentName(component)} : {process} : {info} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            if (_isErrorEnabled)
            {
                return _sender.SendAsync(_channel, ":exclamation:", $"{GetComponentName(component)} : {process} : {exception} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            if (_isFatalErrorEnabled)
            {
                return _sender.SendAsync(_channel, ":no_entry:", $"{GetComponentName(component)} : {process} : {exception} : {context}");
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

        private string GetComponentNamePrefix()
        {
            var sb = new StringBuilder();

            sb.Append($"{AppEnvironment.Name} {AppEnvironment.Version}");

            if (!string.IsNullOrWhiteSpace(AppEnvironment.EnvInfo))
            {
                sb.Append($" : {AppEnvironment.EnvInfo}");
            }

            return sb.ToString();
        }

        private string GetComponentName(string component)
        {
            var sb = new StringBuilder();

            sb.Append(_componentNamePrefix);

            if (AppEnvironment.Name == null || !AppEnvironment.Name.StartsWith(component))
            {
                sb.Append($" : {component}");
            }

            return sb.ToString();
        }
    }
}
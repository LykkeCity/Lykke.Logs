﻿using System;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Common.Log;
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
        private readonly SpamGuard _spamGuard = new SpamGuard();

        private LykkeLogToSlack(
            ISlackNotificationsSender sender,
            string channel,
            LogLevel logLevel,
            bool disableAntiSpam)
        {
            _sender = sender;
            _channel = channel;

            _isInfoEnabled = logLevel.HasFlag(LogLevel.Info);
            _isMonitorEnabled = logLevel.HasFlag(LogLevel.Monitoring);
            _isWarningEnabled = logLevel.HasFlag(LogLevel.Warning);
            _isErrorEnabled = logLevel.HasFlag(LogLevel.Error);
            _isFatalErrorEnabled = logLevel.HasFlag(LogLevel.FatalError);

            _componentNamePrefix = GetComponentNamePrefix();

            if (disableAntiSpam)
                _spamGuard.DisableGuarding();
            else
                SetSpamMutePeriodForLevels(TimeSpan.FromMinutes(1), LogLevel.Warning, LogLevel.Error);
        }

        /// <summary>
        /// Creates logger with, which logs entries of the given <paramref name="logLevel"/>, to the given <paramref name="channel"/>,
        /// using given <paramref name="sender"/> with a flag to disable antispam protection
        /// </summary>
        public static ILog Create(
            ISlackNotificationsSender sender,
            string channel,
            LogLevel logLevel = LogLevel.All,
            bool disableAntiSpam = false)
        {
            return new LykkeLogToSlack(sender, channel, logLevel, disableAntiSpam);
        }

        /// <summary>
        /// Sets spam same mute period for all provided log levels.
        /// </summary>
        /// <param name="mutePeriod">Mute period for spam</param>
        /// <param name="levels">Log levels to be muted in case of spam</param>
        /// <returns>Original instance - for calls chain</returns>
        public LykkeLogToSlack SetSpamMutePeriodForLevels(TimeSpan mutePeriod, params LogLevel[] levels)
        {
            foreach (var level in levels)
            {
                _spamGuard.SetMutePeriod(level, mutePeriod);
            }
            return this;
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isInfoEnabled)
            {
                var message = $"{GetComponentName(component)} : {process} : {info} : {context}";
                return _sender.SendAsync(_channel, ":information_source:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isMonitorEnabled)
            {
                if (_spamGuard.ShouldBeMuted(LogLevel.Monitoring, component, process, info))
                    return Task.CompletedTask;

                var message = $"{GetComponentName(component)} : {process} : {info} : {context}";
                return _sender.SendAsync(_channel, ":loudspeaker:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isWarningEnabled)
            {
                if (_spamGuard.ShouldBeMuted(LogLevel.Warning, component, process, info))
                    return Task.CompletedTask;

                var message = $"{GetComponentName(component)} : {process} : {info} : {context}";
                return _sender.SendAsync(_channel, ":warning:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, Exception ex,
            DateTime? dateTime = null)
        {
            if (_isWarningEnabled)
            {
                if (_spamGuard.ShouldBeMuted(LogLevel.Warning, component, process, $"{info} : {ex?.Message}"))
                    return Task.CompletedTask;

                var message = $"{GetComponentName(component)} : {process} : {ex} : {info} : {context}";
                return _sender.SendAsync(_channel, ":warning:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            if (_isErrorEnabled)
            {
                if (_spamGuard.ShouldBeMuted(LogLevel.Error, component, process, exception.Message))
                    return Task.CompletedTask;

                var message = $"{GetComponentName(component)} : {process} : {exception} : {context}";
                return _sender.SendAsync(_channel, ":exclamation:", message);
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

        public Task WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime = null)
        {
            return WriteWarningAsync(AppEnvironment.Name, process, context, info, ex, dateTime);
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
            if (AppEnvironment.Name == null || !AppEnvironment.Name.StartsWith(component))
                return string.Concat(_componentNamePrefix, " : ", component);
            return _componentNamePrefix;
        }
    }
}
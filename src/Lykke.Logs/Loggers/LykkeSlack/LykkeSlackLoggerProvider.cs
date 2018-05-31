using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    [ProviderAlias("Slack")]
    internal sealed class LykkeSlackLoggerProvider : ILoggerProvider
    {
        [NotNull] private readonly ISpamGuard<Microsoft.Extensions.Logging.LogLevel> _spamGuard;
        [NotNull] private readonly Func<Microsoft.Extensions.Logging.LogLevel, string> _channelResolver;

        [NotNull] private readonly ConcurrentDictionary<string, ILogger> _loggers;
        [NotNull] private readonly ISlackLogEntriesSender _sender;

        public LykkeSlackLoggerProvider(
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName,
            [NotNull] ISpamGuard<Microsoft.Extensions.Logging.LogLevel> spamGuard,
            [NotNull] Func<Microsoft.Extensions.Logging.LogLevel, string> channelResolver)
        {
            _spamGuard = spamGuard ?? throw new ArgumentNullException(nameof(spamGuard));
            _channelResolver = channelResolver ?? throw new ArgumentNullException(nameof(channelResolver));

            _loggers = new ConcurrentDictionary<string, ILogger>();
            _sender = new SlackLogEntriesSender(azureQueueConnectionString, azureQueuesBaseName);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        private ILogger CreateLoggerImplementation(string categoryName)
        {
            return new SpamGuardingLoggerDecorator(
                categoryName,
                new LykkeSlackLogger(_sender, categoryName, _channelResolver),
                _spamGuard);
        }

        public void Dispose()
        {
            _sender.Dispose();
        }
    }
}
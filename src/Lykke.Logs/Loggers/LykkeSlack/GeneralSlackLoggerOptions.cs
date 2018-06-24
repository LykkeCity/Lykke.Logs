using System;
using JetBrains.Annotations;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    internal class GeneralSlackLoggerOptions
    {
        [NotNull]
        public string ConnectionString { get; }

        [NotNull]
        public string BaseQueuesName { get; }

        public GeneralSlackLoggerOptions([NotNull] string connectionString, [NotNull] string baseQueuesName)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            BaseQueuesName = baseQueuesName ?? throw new ArgumentNullException(nameof(baseQueuesName));
        }
    }
}
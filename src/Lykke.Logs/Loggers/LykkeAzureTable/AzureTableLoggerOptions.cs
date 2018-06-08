using System;
using JetBrains.Annotations;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    /// <summary>
    /// Additional options for AzureTable logger
    /// </summary>
    [PublicAPI]
    public class AzureTableLoggerOptions
    {
        /// <summary>
        /// Max time for which entries will be keeped in the in-memory buffer before they will be persisted.
        /// This setting affects max latency before entry will be persisted.
        /// Default value 5 seconds.
        /// </summary>
        public TimeSpan MaxBatchLifetime { get; set; }

        /// <summary>
        /// Amount of entries that triggers batch persisting. Default value is 500
        /// </summary>
        public int BatchSizeThreshold { get; set; }

        internal AzureTableLoggerOptions()
        {
            MaxBatchLifetime = TimeSpan.FromSeconds(5);
            BatchSizeThreshold = 500;
        }
    }
}
using System;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    [PublicAPI]
    public static class LykkeAzureTableLoggerExtensions
    {
        /// <summary>
        /// Adds a azure table logger named 'AzureTable' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="connectionString">Azure Storage connection string</param>
        /// <param name="tableName">Azure Storage table name</param>
        /// <param name="maxBatchLifetime">
        /// Max time for which entries will be keeped in the in-memory buffer before they will be persisted.
        /// This setting affects max latency before entry will be persisted.
        /// Default value 5 seconds
        /// </param>
        /// <param name="batchSizeThreshold">
        /// Amount of entries that triggers batch persisting
        /// </param>
        public static ILoggingBuilder AddLykkeAzureTable(
            [NotNull] this ILoggingBuilder builder, 
            [NotNull] IReloadingManager<string> connectionString, 
            [NotNull] string tableName,
            [CanBeNull] TimeSpan? maxBatchLifetime = null,
            int batchSizeThreshold = 100)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<ILoggerProvider, LykkeAzureTableLoggerProvider>(s => new LykkeAzureTableLoggerProvider(
                connectionString, 
                tableName, 
                maxBatchLifetime, 
                batchSizeThreshold));

            return builder;
        }
    }
}
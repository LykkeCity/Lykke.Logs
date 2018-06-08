using System;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    internal static class LykkeAzureTableLoggerExtensions
    {
        /// <summary>
        /// Adds a azure table logger named 'AzureTable' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILogBuilder"/> to use.</param>
        /// <param name="connectionString">Azure Storage connection string</param>
        /// <param name="tableName">Azure Storage table name</param>
        /// <param name="configure">Allows to perform additional configuration</param>
        public static ILogBuilder AddAzureTable(
            [NotNull] this ILogBuilder builder, 
            [NotNull] IReloadingManager<string> connectionString, 
            [NotNull] string tableName,
            [CanBeNull] Action<AzureTableLoggerOptions> configure = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new AzureTableLoggerOptions();

            configure?.Invoke(options);

            builder.Services.AddSingleton<ILoggerProvider, LykkeAzureTableLoggerProvider>(s => new LykkeAzureTableLoggerProvider(
                connectionString, 
                tableName,
                options));

            return builder;
        }
    }
}
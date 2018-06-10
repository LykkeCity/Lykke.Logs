using System;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.SettingsReader;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    [PublicAPI]
    public static class AzureTableLogFactoryExtensions
    {
        /// <summary>
        /// Adds an Azure table logger
        /// </summary>
        /// <param name="factory">The <see cref="ILogFactory"/> to use.</param>
        /// <param name="tableName">Table name to which logs will be written</param>
        /// <param name="connectionString">Azure storage connection string</param>
        /// <param name="configure">Optional configuration</param>
        public static ILogFactory AddAzureTable(
            [NotNull] this ILogFactory factory,
            [NotNull] IReloadingManager<string> connectionString,
            [NotNull] string tableName,
            Action<AzureTableLoggerOptions> configure = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var options = new AzureTableLoggerOptions();

            configure?.Invoke(options);

            factory.AddProvider(new AzureTableLoggerProvider(connectionString, tableName, options));

            return factory;
        }
    }
}
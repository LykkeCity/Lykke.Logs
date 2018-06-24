using System;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs.Loggers.LykkeAzureTable
{
    internal static class AzureTableLogBuilderExtensions
    {
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

            builder.Services.AddSingleton<ILoggerProvider, AzureTableLoggerProvider>(s => new AzureTableLoggerProvider(
                connectionString,
                tableName,
                options));

            return builder;
        }
    }
}
using System;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Log;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs
{
    /// <summary>
    /// Extension methods to register Lykke health notifications in the app services
    /// </summary>
    [PublicAPI]
    public static class HealthNotificationsExtensions
    {
        /// <summary>
        /// Adds Lykke health notification services to the specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add services to.</param>
        /// <param name="azureQueueConnectionString">Azure Storage connection string</param>
        /// <param name="azureQueuesBaseName">Base name for the Azure Storage queues</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that additional calls can be chained.</returns>
        [NotNull]
        public static IServiceCollection AddLykkeHealthNotifications(
            [NotNull] this IServiceCollection services,
            [NotNull] string azureQueueConnectionString,
            [NotNull] string azureQueuesBaseName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IHealthNotifier, HealthNotifier>(s => new HealthNotifier(
                AppEnvironment.Name,
                AppEnvironment.Version,
                AppEnvironment.EnvInfo,
                // TODO: Regular log factory would be used when legacy methods will be removed from the ILog. 
                // At the moment it can't be used, to prevent circular services dependency
                LastResortLogFactory.Instance,
                azureQueueConnectionString,
                azureQueuesBaseName));

            return services;
        }
    }
}
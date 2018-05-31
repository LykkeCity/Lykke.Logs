using System;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    /// <summary>
    /// Extension methods to register Lykke logging in the app services
    /// </summary>
    [PublicAPI]
    public static class LoggingServiceCollectionExtensions
    {
        [Obsolete("Should be removed with old logging system")]
        internal sealed class BackwardCompatibilityLog
        {
        }

        /// <summary>
        /// Adds Lykke logging services to the specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that additional calls can be chained.</returns>
        [NotNull]
        public static IServiceCollection AddLykkeLogging([NotNull] this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddLykkeLogging(builder => { });
        }

        /// <summary>
        /// Adds Lykke logging services to the specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add services to.</param>
        /// <param name="configure">The <see cref="T:Microsoft.Extensions.Logging.ILoggingBuilder" /> configuration delegate.</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that additional calls can be chained.</returns>
        [NotNull]
        public static IServiceCollection AddLykkeLogging([NotNull] this IServiceCollection services, [NotNull] Action<ILoggingBuilder> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            if (AppEnvironment.EnvInfo == null)
            {
                throw new InvalidOperationException("ENV_INFO environment should be not empty. If you run application in your local machine, please fill up ENV_INFO with your name.");
            }
            if (AppEnvironment.Name == null)
            {
                throw new InvalidOperationException("Application name should be not empty");
            }
            if (AppEnvironment.Version == null)
            {
                throw new InvalidOperationException("Application version should be not empty");
            }

            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogFactory), typeof(LogFactory)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILog), s => s.GetService<ILogFactory>().CreateLog(new BackwardCompatibilityLog())));

            return services.AddLogging(buidler =>
            {
                buidler.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
                buidler.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);

                configure(buidler);
            });
        }
    }
}
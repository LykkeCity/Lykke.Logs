using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs.Loggers.LykkeSanitizing
{
    public static class SanitizingLogBuilderExtensions
    {
        /// <summary>
        /// Adds sensitive pattern that should not be logged. Api keys, private keys and so on.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pattern">Regex to recognize data that should be replaced.</param>
        /// <param name="replacement">String to insert, can be empty string.</param>
        /// <returns><see cref="ILogBuilder"/> instance to continue configuring.</returns>
        public static ILogBuilder AddSanitizingFilter(this ILogBuilder builder, Regex pattern, string replacement)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.Configure<SanitizingOptions>(options => options.Filters.Add(new SanitizingFilter(pattern, replacement)));

            return builder;
        }
    }
}
using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace Lykke.Logs
{
    internal static class ComponentNameHelper
    {
        public static string GetComponentName([NotNull] object component, string suffix = null)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (string.IsNullOrWhiteSpace(suffix))
            {
                return GetBaseComponentName(component);
            }

            return $"{GetBaseComponentName(component)}[{suffix}]";
        }

        private static string GetBaseComponentName(object component)
        {
            if (component is string s)
            {
                return s;
            }

            return TypeNameHelper.GetTypeDisplayName(component.GetType());
        }
    }
}
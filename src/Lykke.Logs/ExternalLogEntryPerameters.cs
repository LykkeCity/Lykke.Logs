using System;
using Lykke.Common;
using Lykke.Common.Log;

namespace Lykke.Logs
{
    internal sealed class ExternalLogEntryPerameters : LogEntryParameters
    {
        public ExternalLogEntryPerameters() : 
            base(
                AppEnvironment.Name, 
                AppEnvironment.Version, 
                AppEnvironment.EnvInfo, 
                string.Empty, 
                string.Empty, 
                1, 
                string.Empty, 
                null, 
                DateTime.UtcNow)
        {
        }
    }
}
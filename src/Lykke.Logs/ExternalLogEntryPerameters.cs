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
                "?", 
                "?", 
                1, 
                "?", 
                null, 
                DateTime.UtcNow)
        {
        }
    }
}
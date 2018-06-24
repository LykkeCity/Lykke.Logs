using System;
using System.Threading.Tasks;

namespace Lykke.Logs.Loggers.LykkeSlack
{
    internal interface ISlackLogEntriesSender : IDisposable
    {
        Task SendAsync(Microsoft.Extensions.Logging.LogLevel level, DateTime moment, string channel, string sender, string message);
    }
}
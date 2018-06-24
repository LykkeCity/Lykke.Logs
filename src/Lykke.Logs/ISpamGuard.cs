using System.Threading.Tasks;

namespace Lykke.Logs
{
    internal interface ISpamGuard<in TLevel>
    {
        Task<bool> ShouldBeMutedAsync(TLevel level, string component, string process);
    }
}
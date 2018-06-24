using System;
using JetBrains.Annotations;

namespace Lykke.Logs
{
    /// <summary>
    /// Log sppam guarding configuration
    /// </summary>
    /// <typeparam name="TLevel"></typeparam>
    [PublicAPI]
    public interface ISpamGuardConfiguration<in TLevel>
    {
        /// <summary>
        /// Disables spam guarding
        /// </summary>
        void DisableGuarding();

        /// <summary>
        /// Set mute period for the specific log level
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="mutePeriod">Period for which log entries of the specified <paramref name="level"/> will be muted</param>
        void SetMutePeriod(TLevel level, TimeSpan mutePeriod);
    }
}
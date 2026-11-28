using System;

namespace MirageXR.FileCache
{
    /// <summary>
    /// <see cref="IClock"/> based on the system time.
    /// </summary>
    public sealed class SystemClock : IClock
    {
        /// <summary>Shared instance; the class has no state.</summary>
        public static readonly SystemClock Instance = new SystemClock();

        /// <inheritdoc/>
        public DateTime UtcNow => DateTime.UtcNow;
    }
}

using System;

namespace MirageXR.FileCache
{
    /// <summary>
    /// <see cref="IClock"/> based on the system time.
    /// </summary>
    public sealed class SystemClock : IClock
    {
        public static readonly SystemClock Instance = new SystemClock();

        public DateTime UtcNow => DateTime.UtcNow;
    }
}

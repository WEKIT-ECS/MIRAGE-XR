using System;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Source of the current time, so that TTL and LRU logic can be tested with a fake clock.
    /// </summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}

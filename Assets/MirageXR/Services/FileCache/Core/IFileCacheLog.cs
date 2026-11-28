namespace MirageXR.FileCache
{
    /// <summary>
    /// Logging sink of the file cache core, so that the core does not depend on UnityEngine.
    /// </summary>
    public interface IFileCacheLog
    {
        /// <summary>Diagnostic message (cache hits/misses, evictions); only relevant when debugging.</summary>
        void Debug(string message);

        /// <summary>Unexpected but handled problem (failed download, unreadable index, file that could not be deleted).</summary>
        void Warning(string message);
    }

    /// <summary>
    /// <see cref="IFileCacheLog"/> that discards all messages.
    /// </summary>
    public sealed class NullFileCacheLog : IFileCacheLog
    {
        /// <summary>Shared instance; the class has no state.</summary>
        public static readonly NullFileCacheLog Instance = new NullFileCacheLog();

        /// <inheritdoc/>
        public void Debug(string message)
        {
        }

        /// <inheritdoc/>
        public void Warning(string message)
        {
        }
    }
}

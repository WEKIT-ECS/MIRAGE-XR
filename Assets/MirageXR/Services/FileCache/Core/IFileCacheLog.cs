namespace MirageXR.FileCache
{
    /// <summary>
    /// Logging sink of the file cache core, so that the core does not depend on UnityEngine.
    /// </summary>
    public interface IFileCacheLog
    {
        void Debug(string message);

        void Warning(string message);
    }

    /// <summary>
    /// <see cref="IFileCacheLog"/> that discards all messages.
    /// </summary>
    public sealed class NullFileCacheLog : IFileCacheLog
    {
        public static readonly NullFileCacheLog Instance = new NullFileCacheLog();

        public void Debug(string message)
        {
        }

        public void Warning(string message)
        {
        }
    }
}

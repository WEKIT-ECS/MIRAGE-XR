namespace MirageXR.FileCache
{
    /// <summary>
    /// <see cref="IFileCacheLog"/> that writes to the Unity console (debug messages only at debug log level).
    /// </summary>
    public sealed class UnityFileCacheLog : IFileCacheLog
    {
        /// <inheritdoc/>
        public void Debug(string message)
        {
            global::Debug.LogDebug(message);
        }

        /// <inheritdoc/>
        public void Warning(string message)
        {
            global::Debug.LogWarning(message);
        }
    }
}

using System;
using System.Threading.Tasks;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Owns the app's disk caches for downloaded models and thumbnails.
    /// </summary>
    public interface IFileCacheService : IDisposable
    {
        event Action Cleared;

        CachedFileProvider AvatarModels { get; }

        CachedFileProvider EnvironmentModels { get; }

        CachedFileProvider Thumbnails { get; }

        /// <summary>Total size of all caches in bytes (from the in-memory index, no disk access).</summary>
        long TotalSizeBytes { get; }

        Task InitializeAsync();

        /// <summary>Empties all caches. Files in use are deleted as soon as they are released.</summary>
        Task ClearAllAsync();
    }
}

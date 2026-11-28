using System;
using System.Threading.Tasks;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Owns the app's disk caches for downloaded models and thumbnails.
    /// </summary>
    public interface IFileCacheService : IDisposable
    {
        /// <summary>Raised after <see cref="ClearAllAsync"/> has emptied all caches.</summary>
        event Action Cleared;

        /// <summary>Avatar models (glTF/GLB) for sharing avatars and virtual instructors.</summary>
        CachedFileProvider AvatarModels { get; }

        /// <summary>Environment (room twin) models; the largest files, so this cache has the highest size limit.</summary>
        CachedFileProvider EnvironmentModels { get; }

        /// <summary>Preview images of avatars and environments.</summary>
        CachedFileProvider Thumbnails { get; }

        /// <summary>Total size of all caches in bytes (from the in-memory index, no disk access).</summary>
        long TotalSizeBytes { get; }

        /// <summary>Initializes all caches (see <see cref="IFileCache.InitializeAsync"/>). Call it once before the providers are used.</summary>
        Task InitializeAsync();

        /// <summary>Empties all caches. Files in use are deleted as soon as they are released.</summary>
        Task ClearAllAsync();

        /// <summary>
        /// Writes pending index changes of all caches to disk synchronously. Call it when the app is paused or
        /// loses focus: a suspended app can be killed without <see cref="IDisposable.Dispose"/> being called.
        /// </summary>
        void Flush();
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Creates and owns the disk caches for avatar models, environment (room twin) models and thumbnails.
    /// Each cache lives in its own sub folder of the given base directory.
    /// </summary>
    public sealed class FileCacheService : IFileCacheService
    {
        private const long MegaByte = 1024L * 1024L;

        private const long EnvironmentModelsMaxSize = 500 * MegaByte;
        private const long AvatarModelsMaxSize = 150 * MegaByte;
        private const long ThumbnailsMaxSize = 50 * MegaByte;

        private const int ModelDownloadConcurrency = 2;
        private const int ThumbnailDownloadConcurrency = 4;

        private static readonly TimeSpan TimeToLive = TimeSpan.FromDays(7);

        private readonly IFileCache[] _caches;
        private readonly IFileCacheLog _log;

        /// <param name="baseDirectory">Folder for all caches, e.g. <c>Application.temporaryCachePath/FileCache</c>.</param>
        public FileCacheService(string baseDirectory, IRemoteFileFetcher fetcher, IClock clock = null, IFileCacheLog log = null)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new ArgumentException("A base directory is required.", nameof(baseDirectory));
            }

            _log = log ?? NullFileCacheLog.Instance;

            var environmentCache = CreateCache("EnvironmentModels", EnvironmentModelsMaxSize);
            var avatarCache = CreateCache("AvatarModels", AvatarModelsMaxSize);
            var thumbnailCache = CreateCache("Thumbnails", ThumbnailsMaxSize);
            _caches = new IFileCache[] { environmentCache, avatarCache, thumbnailCache };

            EnvironmentModels = new CachedFileProvider(environmentCache, fetcher, ModelDownloadConcurrency, clock, _log);
            AvatarModels = new CachedFileProvider(avatarCache, fetcher, ModelDownloadConcurrency, clock, _log);
            Thumbnails = new CachedFileProvider(thumbnailCache, fetcher, ThumbnailDownloadConcurrency, clock, _log);

            IFileCache CreateCache(string name, long maxSize)
            {
                var options = new FileCacheOptions(name, Path.Combine(baseDirectory, name), maxSize, TimeToLive);
                return new FileCache(options, clock, _log);
            }
        }

        public event Action Cleared;

        public CachedFileProvider AvatarModels { get; }

        public CachedFileProvider EnvironmentModels { get; }

        public CachedFileProvider Thumbnails { get; }

        public long TotalSizeBytes => _caches.Sum(cache => cache.SizeBytes);

        /// <summary>Initializes all caches. Never throws: a cache that fails to initialize starts empty.</summary>
        public Task InitializeAsync()
        {
            return Task.WhenAll(_caches.Select(InitializeCacheAsync));
        }

        public async Task ClearAllAsync()
        {
            await Task.WhenAll(_caches.Select(cache => cache.ClearAsync()));
            Cleared?.Invoke();
        }

        public void Flush()
        {
            foreach (var cache in _caches)
            {
                cache.Flush();
            }
        }

        public void Dispose()
        {
            foreach (var cache in _caches)
            {
                cache.Dispose();
            }
        }

        private async Task InitializeCacheAsync(IFileCache cache)
        {
            try
            {
                await cache.InitializeAsync();
            }
            catch (Exception e)
            {
                _log.Warning($"[FileCache:{cache.Options.Name}] initialization failed: {e.Message}");
            }
        }
    }
}

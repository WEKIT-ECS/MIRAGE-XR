using System;
using System.Threading.Tasks;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Size-bounded, file-type-independent disk cache with LRU eviction.
    /// All methods are thread-safe; disk I/O runs on the thread pool.
    /// </summary>
    public interface IFileCache : IDisposable
    {
        /// <summary>Configuration of this cache (name, folder, size limit, TTL).</summary>
        FileCacheOptions Options { get; }

        /// <summary>Total size of all cached files in bytes.</summary>
        long SizeBytes { get; }

        /// <summary>Number of cached entries.</summary>
        int Count { get; }

        /// <summary>Loads the index, removes interrupted downloads and reconciles the index with the directory.</summary>
        Task InitializeAsync();

        /// <summary>
        /// Returns a lease on the cached file of <paramref name="key"/> and marks it as recently used,
        /// or <c>null</c> if there is no (existing) file for the key.
        /// </summary>
        Task<FileLease> TryAcquireAsync(string key);

        /// <summary>
        /// Ensures the cache's temp folder exists and returns a new path inside it (same volume, so commits are atomic moves).
        /// </summary>
        Task<string> CreateTempFilePathAsync(string fileExtension);

        /// <summary>Deletes a temp file that will not be committed (failed or cancelled download).</summary>
        Task DiscardTempFileAsync(string tempFilePath);

        /// <summary>
        /// Moves the downloaded temp file into the cache (replacing an older version of the key), evicts least recently
        /// used entries if needed and returns a lease on the committed file. A file larger than
        /// <see cref="FileCacheOptions.MaxSizeBytes"/> is not cached; its lease deletes it when disposed.
        /// </summary>
        Task<FileLease> CommitAsync(string key, string tempFilePath, string eTag, string lastModified);

        /// <summary>Records that the server confirmed the leased version of the file (e.g. HTTP 304).</summary>
        void MarkValidated(FileLease lease, string eTag, string lastModified);

        /// <summary>Removes the entry of <paramref name="key"/>; a leased file is deleted once released.</summary>
        Task RemoveAsync(string key);

        /// <summary>Removes all entries; leased files are deleted once released.</summary>
        Task ClearAsync();

        /// <summary>
        /// Writes pending index changes to disk synchronously (a no-op when nothing changed). Meant for the moment
        /// the app is paused/backgrounded: mobile OSes may kill a suspended app without any further callback.
        /// </summary>
        void Flush();
    }
}

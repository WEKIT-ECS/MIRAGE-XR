using System;

namespace MirageXR.FileCache
{
    /// <summary>
    /// A local file served by a <see cref="CachedFileProvider"/>. The file stays available until this object is
    /// disposed, so dispose it right after the file was read.
    /// </summary>
    public sealed class CachedFile : IDisposable
    {
        private readonly FileLease _lease;

        internal CachedFile(string key, FileLease lease, bool fromCache, bool isStale)
        {
            Key = key;
            _lease = lease;
            FromCache = fromCache;
            IsStale = isStale;
        }

        public string Key { get; }

        /// <summary>Absolute path of the local file.</summary>
        public string Path => _lease.Path;

        /// <summary>True if the file was served from disk without downloading it.</summary>
        public bool FromCache { get; }

        /// <summary>True if the file is past its TTL and could not be revalidated (e.g. offline).</summary>
        public bool IsStale { get; }

        public void Dispose()
        {
            _lease.Dispose();
        }
    }
}

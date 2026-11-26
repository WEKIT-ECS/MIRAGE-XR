using System;
using System.Threading;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Owner side of leases; implemented by <see cref="FileCache"/>.
    /// </summary>
    internal interface ILeaseOwner
    {
        FileLease AddLease(string path, CacheEntry entry);

        void ReleaseLease(string path);
    }

    /// <summary>
    /// Keeps a file of a <see cref="IFileCache"/> alive: a leased file is neither evicted nor deleted
    /// (deletion is deferred until the last lease is disposed). Dispose it as soon as the file was read.
    /// </summary>
    public sealed class FileLease : IDisposable
    {
        private readonly ILeaseOwner _owner;
        private int _isDisposed;

        internal FileLease(ILeaseOwner owner, string path, CacheEntry entry)
        {
            _owner = owner;
            Path = path;
            Entry = entry;
        }

        /// <summary>Absolute path of the local file.</summary>
        public string Path { get; }

        /// <summary>Snapshot of the index entry, or <c>null</c> if the file is not part of the cache (too large to cache).</summary>
        public CacheEntry Entry { get; }

        public bool IsCached => Entry != null;

        public bool IsDisposed => Volatile.Read(ref _isDisposed) != 0;

        /// <summary>Creates an independent lease on the same file.</summary>
        public FileLease Duplicate()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FileLease));
            }

            return _owner.AddLease(Path, Entry?.Clone());
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                _owner.ReleaseLease(Path);
            }
        }
    }
}

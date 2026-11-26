using System;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Configuration of a single <see cref="FileCache"/>.
    /// </summary>
    public sealed class FileCacheOptions
    {
        public FileCacheOptions(string name, string rootDirectory, long maxSizeBytes, TimeSpan timeToLive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A cache name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentException("A root directory is required.", nameof(rootDirectory));
            }

            if (maxSizeBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSizeBytes), "The maximum size must be positive.");
            }

            if (timeToLive < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeToLive), "The time to live must not be negative.");
            }

            Name = name;
            RootDirectory = rootDirectory;
            MaxSizeBytes = maxSizeBytes;
            TimeToLive = timeToLive;
        }

        /// <summary>Name of the cache, used in log messages.</summary>
        public string Name { get; }

        /// <summary>Directory that exclusively belongs to this cache.</summary>
        public string RootDirectory { get; }

        /// <summary>Upper bound of the total size of all cached files. Least recently used files are evicted beyond it.</summary>
        public long MaxSizeBytes { get; }

        /// <summary>How long an entry is used without asking the server again.</summary>
        public TimeSpan TimeToLive { get; }

        /// <summary>Delay used to coalesce several index changes into one write of the manifest.</summary>
        public TimeSpan ManifestSaveDelay { get; set; } = TimeSpan.FromSeconds(2);
    }
}

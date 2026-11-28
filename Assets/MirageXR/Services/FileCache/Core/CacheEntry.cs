using System;
using Newtonsoft.Json;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Index entry of a cached file. Instances handed out by the cache are snapshots.
    /// </summary>
    public sealed class CacheEntry
    {
        /// <summary>Cache key (normalized URL without credentials, see <see cref="CacheRequest.Key"/>).</summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>File name inside the cache's root directory.</summary>
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        /// <summary>Size of the cached file in bytes.</summary>
        [JsonProperty("sizeBytes")]
        public long SizeBytes { get; set; }

        /// <summary>When the file was last served; used for LRU eviction and the unused-entry cleanup at startup.</summary>
        [JsonProperty("lastAccessUtc")]
        public DateTime LastAccessUtc { get; set; }

        /// <summary>When the server last confirmed (or delivered) this version of the file.</summary>
        [JsonProperty("validatedUtc")]
        public DateTime ValidatedUtc { get; set; }

        /// <summary>ETag the server sent for this version, or <c>null</c>; used for conditional revalidation.</summary>
        [JsonProperty("eTag")]
        public string ETag { get; set; }

        /// <summary>Last-Modified value the server sent for this version, or <c>null</c>; used for conditional revalidation.</summary>
        [JsonProperty("lastModified")]
        public string LastModified { get; set; }

        /// <summary>True if a stale entry can be revalidated with a conditional request instead of a full download.</summary>
        [JsonIgnore]
        public bool HasValidators => !string.IsNullOrEmpty(ETag) || !string.IsNullOrEmpty(LastModified);

        /// <summary>Creates an independent copy, so callers can never modify the cache's index.</summary>
        public CacheEntry Clone()
        {
            return (CacheEntry)MemberwiseClone();
        }
    }
}

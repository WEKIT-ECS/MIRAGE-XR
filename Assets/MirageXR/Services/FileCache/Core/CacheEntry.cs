using System;
using Newtonsoft.Json;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Index entry of a cached file. Instances handed out by the cache are snapshots.
    /// </summary>
    public sealed class CacheEntry
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>File name inside the cache's root directory.</summary>
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("sizeBytes")]
        public long SizeBytes { get; set; }

        [JsonProperty("lastAccessUtc")]
        public DateTime LastAccessUtc { get; set; }

        /// <summary>When the server last confirmed (or delivered) this version of the file.</summary>
        [JsonProperty("validatedUtc")]
        public DateTime ValidatedUtc { get; set; }

        [JsonProperty("eTag")]
        public string ETag { get; set; }

        [JsonProperty("lastModified")]
        public string LastModified { get; set; }

        [JsonIgnore]
        public bool HasValidators => !string.IsNullOrEmpty(ETag) || !string.IsNullOrEmpty(LastModified);

        public CacheEntry Clone()
        {
            return (CacheEntry)MemberwiseClone();
        }
    }
}

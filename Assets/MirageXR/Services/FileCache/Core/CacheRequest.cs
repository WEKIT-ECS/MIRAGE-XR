using System;
using System.Collections.Generic;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Describes a remote file that should be served through a <see cref="CachedFileProvider"/>.
    /// </summary>
    public sealed class CacheRequest
    {
        private static readonly IReadOnlyDictionary<string, string> NoHeaders = new Dictionary<string, string>();

        /// <param name="url">Absolute URL of the file.</param>
        /// <param name="headers">Request headers (e.g. authorization). They are not part of the cache key.</param>
        /// <param name="fileExtension">Extension of the cached file (e.g. ".glb"), only for readability on disk.</param>
        /// <param name="key">Cache key; defaults to the normalized URL.</param>
        public CacheRequest(string url, IReadOnlyDictionary<string, string> headers = null, string fileExtension = null, string key = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("A URL is required.", nameof(url));
            }

            Url = url;
            Headers = headers ?? NoHeaders;
            FileExtension = fileExtension;
            Key = key ?? CreateKey(url);
        }

        /// <summary>Absolute URL the file is downloaded from.</summary>
        public string Url { get; }

        /// <summary>Request headers sent with every download or revalidation; never <c>null</c>.</summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>Extension of the cached file (e.g. ".glb"), or <c>null</c>.</summary>
        public string FileExtension { get; }

        /// <summary>Identifies the file in the cache: requests with the same key share one entry and one download.</summary>
        public string Key { get; }

        /// <summary>
        /// Normalizes a URL into a cache key. Scheme, host and port are part of it, so different servers
        /// (e.g. production and development ports) never share entries.
        /// </summary>
        public static string CreateKey(string url)
        {
            return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ? uri.AbsoluteUri : url.Trim();
        }
    }
}

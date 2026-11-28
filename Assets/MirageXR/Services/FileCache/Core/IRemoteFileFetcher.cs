using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Downloads a remote file directly into a local file.
    /// </summary>
    public interface IRemoteFileFetcher
    {
        /// <summary>
        /// Downloads <see cref="FetchRequest.Url"/> into <see cref="FetchRequest.DestinationPath"/>.
        /// Network and server errors are reported as <see cref="FetchStatus.Failed"/>; cancellation throws an
        /// <see cref="System.OperationCanceledException"/>.
        /// </summary>
        Task<FetchResult> FetchToFileAsync(FetchRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Parameters of a single download by an <see cref="IRemoteFileFetcher"/>.
    /// </summary>
    public sealed class FetchRequest
    {
        /// <param name="url">Absolute URL of the file.</param>
        /// <param name="headers">Additional request headers (e.g. authorization), or <c>null</c>.</param>
        /// <param name="destinationPath">Local file the response body is written to.</param>
        /// <param name="ifNoneMatch">ETag of the cached version for a conditional request, or <c>null</c>.</param>
        /// <param name="ifModifiedSince">Last-Modified value of the cached version for a conditional request, or <c>null</c>.</param>
        public FetchRequest(string url, IReadOnlyDictionary<string, string> headers, string destinationPath, string ifNoneMatch, string ifModifiedSince)
        {
            Url = url;
            Headers = headers;
            DestinationPath = destinationPath;
            IfNoneMatch = ifNoneMatch;
            IfModifiedSince = ifModifiedSince;
        }

        /// <summary>Absolute URL of the file.</summary>
        public string Url { get; }

        /// <summary>Additional request headers, or <c>null</c>.</summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>Local file the response body is written to (a path in the cache's temp folder).</summary>
        public string DestinationPath { get; }

        /// <summary>ETag of the cached version for a conditional request, or <c>null</c>.</summary>
        public string IfNoneMatch { get; }

        /// <summary>Last-Modified value of the cached version for a conditional request, or <c>null</c>.</summary>
        public string IfModifiedSince { get; }

        /// <summary>True if validators of a cached version are sent, so the server may answer "not modified" (HTTP 304).</summary>
        public bool IsConditional => !string.IsNullOrEmpty(IfNoneMatch) || !string.IsNullOrEmpty(IfModifiedSince);
    }
}

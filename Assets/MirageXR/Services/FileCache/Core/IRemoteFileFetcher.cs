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

    public sealed class FetchRequest
    {
        public FetchRequest(string url, IReadOnlyDictionary<string, string> headers, string destinationPath, string ifNoneMatch, string ifModifiedSince)
        {
            Url = url;
            Headers = headers;
            DestinationPath = destinationPath;
            IfNoneMatch = ifNoneMatch;
            IfModifiedSince = ifModifiedSince;
        }

        public string Url { get; }

        public IReadOnlyDictionary<string, string> Headers { get; }

        public string DestinationPath { get; }

        /// <summary>ETag of the cached version for a conditional request, or <c>null</c>.</summary>
        public string IfNoneMatch { get; }

        /// <summary>Last-Modified value of the cached version for a conditional request, or <c>null</c>.</summary>
        public string IfModifiedSince { get; }

        public bool IsConditional => !string.IsNullOrEmpty(IfNoneMatch) || !string.IsNullOrEmpty(IfModifiedSince);
    }
}

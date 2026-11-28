using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using UnityEngine.Networking;

namespace MirageXR.FileCache
{
    /// <summary>
    /// <see cref="IRemoteFileFetcher"/> that streams the response body directly to disk with a
    /// <see cref="DownloadHandlerFile"/>, so large files never end up in managed memory. Must be called on the main thread.
    /// </summary>
    public sealed class UnityWebRequestFileFetcher : IRemoteFileFetcher
    {
        private const long HttpNotModified = 304;

        /// <inheritdoc/>
        public async Task<FetchResult> FetchToFileAsync(FetchRequest request, CancellationToken cancellationToken)
        {
            using var webRequest = new UnityWebRequest(request.Url, UnityWebRequest.kHttpVerbGET);
            webRequest.downloadHandler = new DownloadHandlerFile(request.DestinationPath) { removeFileOnAbort = true };
            webRequest.certificateHandler = new BypassCertificateHandler();

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(request.IfNoneMatch))
            {
                webRequest.SetRequestHeader("If-None-Match", request.IfNoneMatch);
            }

            if (!string.IsNullOrEmpty(request.IfModifiedSince))
            {
                webRequest.SetRequestHeader("If-Modified-Since", request.IfModifiedSince);
            }

            try
            {
                await webRequest.SendWebRequest().WithCancellation(cancellationToken);
            }
            catch (UnityWebRequestException)
            {
                // evaluated below via result and response code
            }

            var eTag = webRequest.GetResponseHeader("ETag");
            var lastModified = webRequest.GetResponseHeader("Last-Modified");

            if (webRequest.responseCode == HttpNotModified)
            {
                return FetchResult.NotModified(eTag, lastModified);
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                return FetchResult.Ok(eTag, lastModified);
            }

            return FetchResult.Failed($"{webRequest.result} (HTTP {webRequest.responseCode}): {webRequest.error}");
        }
    }
}

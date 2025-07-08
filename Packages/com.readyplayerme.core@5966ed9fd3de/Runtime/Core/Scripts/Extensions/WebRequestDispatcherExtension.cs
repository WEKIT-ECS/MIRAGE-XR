using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="WebRequestDispatcher"/> class,
    /// download into a file or memory, download texture or send a post request with payload.
    /// </summary>
    public static class WebRequestDispatcherExtension
    {
        private const int TIMEOUT = 20;
        private const string NO_INTERNET_CONNECTION = "No internet connection.";
        private const string CLOUDFRONT_IDENTIFIER = "cloudfront";

        private static bool HasInternetConnection => Application.internetReachability != NetworkReachability.NotReachable;

        /// <summary>
        /// This asynchronous method makes GET request to the <paramref name="url" /> and returns the data in the
        /// <see cref="IResponse" />.
        /// </summary>
        /// <param name="webRequestDispatcher">WebRequestDispatcher object</param>
        /// <param name="url">The URL to make the <see cref="UnityWebRequest" /> to.</param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <param name="timeout">The number of seconds to wait for the WebRequest to finish before aborting.</param>
        /// <returns>A <see cref="IResponse" /> if successful otherwise it will throw an exception.</returns>
        public static async Task<T> DownloadIntoMemory<T>(this WebRequestDispatcher webRequestDispatcher, string url, CancellationToken token,
            int timeout = TIMEOUT) where T : IResponse, new()
        {
            if (!HasInternetConnection)
            {
                throw new CustomException(FailureType.NoInternetConnection, NO_INTERNET_CONNECTION);
            }

            IDictionary<string, string> headers = new Dictionary<string, string>();
            if (!url.Contains(CLOUDFRONT_IDENTIFIER)) // Required to prevent CORS errors in WebGL
            {
                headers = CommonHeaders.GetAnalyticsHeaders();
            }

            headers.Add(CommonHeaders.GetAppIdHeader());

            webRequestDispatcher.Timeout = timeout;
            var response = await webRequestDispatcher.SendRequest<T>(url, HttpMethod.GET, headers, ctx: token);
            token.ThrowCustomExceptionIfCancellationRequested();

            if (!response.IsSuccess)
            {
                throw new CustomException(FailureType.DownloadError, response.Error);
            }

            return response;
        }

        /// <summary>
        /// This asynchronous method makes a web request to the <paramref name="url" /> and stores the data into a file at
        /// <paramref name="path" />.
        /// </summary>
        /// <param name="webRequestDispatcher">WebRequestDispatcher object</param>
        /// <param name="url">The URL to make the <see cref="UnityWebRequest" /> to.</param>
        /// <param name="path">Where to create the file and store the response data.</param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <param name="timeout">The number of seconds to wait for the WebRequest to finish before aborting.</param>
        /// <returns>A <see cref="ResponseFile" /> with the data included if successful otherwise it will throw an exception.</returns>
        public static async Task<ResponseFile> DownloadIntoFile(this WebRequestDispatcher webRequestDispatcher, string url, string path,
            CancellationToken token, int timeout = TIMEOUT)
        {
            if (!HasInternetConnection)
            {
                throw new CustomException(FailureType.NoInternetConnection, NO_INTERNET_CONNECTION);
            }

            var downloadHandler = new DownloadHandlerFile(path);
            downloadHandler.removeFileOnAbort = true;

            IDictionary<string, string> headers = new Dictionary<string, string>();
            if (!url.Contains(CLOUDFRONT_IDENTIFIER)) // Required to prevent CORS errors in WebGL
            {
                headers = CommonHeaders.GetAnalyticsHeaders();
            }

            headers.Add(CommonHeaders.GetAppIdHeader());

            webRequestDispatcher.Timeout = timeout;
            var response = await webRequestDispatcher.SendRequest<ResponseFile>(url, HttpMethod.GET, headers, downloadHandler: downloadHandler,
                ctx: token);
            token.ThrowCustomExceptionIfCancellationRequested();
            if (!response.IsSuccess)
            {
                throw new CustomException(FailureType.DownloadError, response.Error);
            }

            await response.ReadFile(path, token);
            return response;
        }

        /// <summary>
        /// This asynchronous method makes a web request to the <paramref name="url" /> and returns the data as a
        /// <see cref="Texture2D" />.
        /// </summary>
        /// <param name="webRequestDispatcher">WebRequestDispatcher object</param>
        /// <param name="url">The URL to make the <see cref="UnityWebRequest" /> to.</param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <param name="timeout">Used to set how long to wait before the request will time out</param>
        /// <returns>The response data as a <see cref="Texture2D" /> if successful otherwise it will throw an exception.</returns>
        public static async Task<Texture2D> DownloadTexture(this WebRequestDispatcher webRequestDispatcher, string url, CancellationToken token = default,
            int timeout = TIMEOUT)
        {
            if (!HasInternetConnection)
            {
                throw new CustomException(FailureType.NoInternetConnection, NO_INTERNET_CONNECTION);
            }

            webRequestDispatcher.Timeout = timeout;
            var response = await webRequestDispatcher.SendRequest<ResponseTexture>(url, HttpMethod.GET, downloadHandler: new DownloadHandlerTexture(),
                ctx: token);

            token.ThrowCustomExceptionIfCancellationRequested();

            if (!response.IsSuccess)
            {
                throw new CustomException(FailureType.DownloadError, response.Error);
            }

            return response.Texture;
        }
    }
}

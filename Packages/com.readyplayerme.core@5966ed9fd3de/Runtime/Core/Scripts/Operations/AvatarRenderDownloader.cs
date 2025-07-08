using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class is responsible for requesting and downloading a 2D render of an avatar from a URL.
    /// </summary>
    public class AvatarRenderDownloader : IOperation<AvatarContext>
    {
        private const string TAG = nameof(AvatarRenderDownloader);
        private const string AVATAR_RENDER_DOWNLOADED = "Avatar Render Downloaded";

        /// <summary>
        /// Can be used to set the Timeout (in seconds) used by the <see cref="WebRequestDispatcherExtension" /> when making the web request.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// An <see cref="Action" /> callback that can be used to subscribe to <see cref="WebRequestDispatcherExtension" />
        /// <c>ProgressChanged</c> events.
        /// </summary>
        public Action<float> ProgressChanged { get; set; }

        /// <summary>
        /// Executes the operation to request and download the 2D render and returns the updated context.
        /// </summary>
        /// <param name="context">A container for all the data related to the Avatar model.</param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <returns>The updated <c>AvatarContext</c>.</returns>
        public async Task<AvatarContext> Execute(AvatarContext context, CancellationToken token)
        {
            try
            {
                var renderUrl = RenderParameterProcessor.GetRenderUrl(context);
                context.Data = await RequestAvatarRender(renderUrl, token);
                SDKLogger.Log(TAG, AVATAR_RENDER_DOWNLOADED);
                return context;
            }
            catch (CustomException exception)
            {
                if (exception.FailureType != FailureType.NoInternetConnection)
                {
                    throw new CustomException(FailureType.AvatarRenderError, exception.Message);
                }

                throw;
            }
        }

        /// <summary>
        /// Requests an avatar render URL asynchronously
        /// </summary>
        /// <param name="url">The url for avatar render texture.</param>
        /// <param name="token">Can be used to cancel the operation.</param>
        private async Task<Texture2D> RequestAvatarRender(string url, CancellationToken token = new CancellationToken())
        {
            var webRequestDispatcher = new WebRequestDispatcher();
            webRequestDispatcher.ProgressChanged += ProgressChanged;
            return await webRequestDispatcher.DownloadTexture(url, token);
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class is responsible for making a request and downloading an avatar from a URL.
    /// </summary>
    public class AvatarDownloader : IOperation<AvatarContext>
    {
        private const string TAG = nameof(AvatarDownloader);
        private const string DOWNLOADING_AVATAR_INTO_MEMORY = "Downloading avatar into memory.";
        private const string LOADING_MODEL_FROM_CACHE = "Loading model from cache.";

        /// If true the avatar will download into memory instead of a local file.
        private readonly bool downloadInMemory;

        /// <summary>
        /// The <c>AvatarDownloader</c> constructor can be used to set <c>downloadInMemory</c>.
        /// </summary>
        /// <param name="downloadInMemory">
        /// If true <c>AvatarDownloader</c> will download the avatar into memory instead of into a
        /// file that is stored locally.
        /// </param>
        public AvatarDownloader(bool downloadInMemory = false)
        {
            this.downloadInMemory = downloadInMemory;
        }

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
        /// Executes the operation to download the avatar from <c>AvatarContext.AvatarUri</c> and returns the updated context.
        /// </summary>
        /// <param name="context">A container for all the data related to the Avatar model.</param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <returns>The <c>AvatarContext</c> with the downloaded bytes included.</returns>
        public async Task<AvatarContext> Execute(AvatarContext context, CancellationToken token)
        {
            if (context.AvatarUri.Equals(default(AvatarUri)))
            {
                throw new InvalidDataException($"Expected cast {typeof(string)} instead got ");
            }

            DirectoryUtility.ValidateAvatarSaveDirectory(context.AvatarUri.Guid, context.ParametersHash);

            if ((!context.IsUpdateRequired || Application.internetReachability == NetworkReachability.NotReachable)
                && File.Exists(context.AvatarUri.LocalModelPath))
            {
                SDKLogger.Log(TAG, LOADING_MODEL_FROM_CACHE);
                context.Bytes = File.ReadAllBytes(context.AvatarUri.LocalModelPath);
                return context;
            }

            if (context.IsUpdateRequired)
            {
                AvatarCache.DeleteAvatarModel(context.AvatarUri.Guid, context.ParametersHash);
            }

            if (!context.AvatarCachingEnabled || downloadInMemory)
            {
                context.Bytes = await DownloadIntoMemory(context.AvatarUri.ModelUrl, context.AvatarConfig, token);
                return context;
            }

            context.Bytes = await DownloadIntoFile(context.AvatarUri.ModelUrl, context.AvatarUri.LocalModelPath, context.AvatarConfig, token);
            return context;
        }

        /// <summary>
        /// An asynchronous task that downloads the avatar into memory and returns the data as a <c>byte[]</c>.
        /// </summary>
        /// <param name="url">The avatar .glb url.</param>
        /// <param name="avatarConfig">
        /// The <see cref="AvatarConfig" /> can be used to adjust the configuration of the downloaded
        /// avatar using the Avatar API. By default is set to null.
        /// </param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <returns>A <c>byte[]</c> that holds the data of the downloaded avatar .glb file.</returns>
        public async Task<byte[]> DownloadIntoMemory(string url, AvatarConfig avatarConfig = null, CancellationToken token = new CancellationToken())
        {
            if (avatarConfig)
            {
                var parameters = AvatarConfigProcessor.ProcessAvatarConfiguration(avatarConfig);
                url += $"?{parameters}";
                SDKLogger.Log(TAG, $"Download URL with parameters: {url}");
            }

            SDKLogger.Log(TAG, DOWNLOADING_AVATAR_INTO_MEMORY);

            var dispatcher = new WebRequestDispatcher();
            dispatcher.ProgressChanged = ProgressChanged;

            try
            {
                var response = await dispatcher.DownloadIntoMemory<ResponseData>(url, token, Timeout);
                return response.Data;
            }
            catch (CustomException exception)
            {
                if (exception.FailureType == FailureType.NoInternetConnection)
                {
                    throw;
                }

                throw Fail($"Failed to download glb model into memory. {exception}");
            }
        }

        /// <summary>
        /// An asynchronous task that downloads the avatar into a file stored locally and returns the data as a <c>byte[]</c>.
        /// </summary>
        /// <param name="url">The avatar .glb url</param>
        /// <param name="path">Path to file to be written </param>
        /// <param name="avatarConfig">
        /// The <see cref="AvatarConfig" /> can be used to adjust the configuration of the downloaded
        /// avatar using the Avatar API. By default is set to null.
        /// </param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <returns>A <c>byte[]</c> that holds the data of the downloaded avatar .glb file.</returns>
        public async Task<byte[]> DownloadIntoFile(string url, string path, AvatarConfig avatarConfig = null, CancellationToken token = new CancellationToken())
        {
            if (avatarConfig)
            {
                var parameters = AvatarConfigProcessor.ProcessAvatarConfiguration(avatarConfig);
                url += $"?{parameters}";
                SDKLogger.Log(TAG, $"Download URL with parameters: {url}");
            }

            SDKLogger.Log(TAG, $"Downloading avatar into file at {path}");

            var dispatcher = new WebRequestDispatcher();
            dispatcher.ProgressChanged = ProgressChanged;

            try
            {
                var response = await dispatcher.DownloadIntoFile(url, path, token, Timeout);
                return response.Data;
            }
            catch (CustomException exception)
            {
                if (exception.FailureType == FailureType.NoInternetConnection)
                {
                    throw;
                }

                throw Fail($"Failed to download glb model into file. {exception}");
            }
        }

        /// <summary>
        /// A method used to throw <c>ModelDownloadError</c> exceptions.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>The <c>Exception</c>.</returns>
        private Exception Fail(string message)
        {
            SDKLogger.Log(TAG, message);
            throw new CustomException(FailureType.ModelDownloadError, message);
        }
    }
}

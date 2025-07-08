using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class is responsible for handling the avatar meta data .json file download, save, and parsing functionality.
    /// </summary>
    public class MetadataDownloader : IOperation<AvatarContext>
    {
        private const string TAG = nameof(MetadataDownloader);

        /// <summary>
        /// Can be used to set the Timeout (in seconds) used by the <see cref="WebRequestDispatcherExtension" /> when making the web request.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// An <see cref="Action" /> callback that can be used to subscribe to <see cref="WebRequestDispatcherExtension" />
        /// <c>ProgressChanged</c> events.
        /// </summary>
        public Action<float> ProgressChanged { get; set; }

        private const string METADATA_TIME_FORMAT = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
        private const string DOWNLOADING_METADATA_INTO_MEMORY = "Downloading metadata into memory.";
        private const string FAILED_TO_PARSE_METADATA_UNEXPECTED_BODY_TYPE = "Failed to parse metadata. Unexpected body type.";

        /// <summary>
        /// Executes the operation to download the avatar and save to file if saving is enabled.
        /// </summary>
        /// <param name="context">A container for all the data related to the Avatar model.</param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <returns>The updated <see cref="AvatarContext" />.</returns>
        public async Task<AvatarContext> Execute(AvatarContext context, CancellationToken token)
        {
            if (context.AvatarUri.Equals(default(AvatarUri)))
            {
                throw new InvalidDataException($"Expected cast {typeof(string)}");
            }
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                context.Metadata = AvatarMetadata.LoadFromFile(context.AvatarUri.LocalMetadataPath);
            }
            else
            {
                AvatarMetadata metadata = await Download(context.AvatarUri.MetadataUrl, token);
                context = UpdateContext(context, metadata);
                if (context.IsUpdateRequired && context.AvatarCachingEnabled)
                {
                    SaveMetadataToFile(context);
                }
            }

            return context;
        }

        private AvatarContext UpdateContext(AvatarContext avatarContext, AvatarMetadata metadata)
        {
            avatarContext.Metadata = metadata;
            avatarContext.IsUpdateRequired = IsUpdateRequired(avatarContext);
            return avatarContext;
        }

        /// <summary>
        /// Downloads the avatar meta data and parses the response.
        /// </summary>
        /// <param name="url">The URL of the avatar metadata ending in <c>.json</c></param>
        /// <param name="token">Can be used to cancel the operation.</param>
        /// <returns>The avatar metadata as a <see cref="AvatarMetadata" /> structure.</returns>
        public async Task<AvatarMetadata> Download(string url, CancellationToken token = new CancellationToken())
        {
            SDKLogger.Log(TAG, DOWNLOADING_METADATA_INTO_MEMORY);
            var dispatcher = new WebRequestDispatcher();
            dispatcher.ProgressChanged += ProgressChanged;

            try
            {
#if UNITY_WEBGL
                // add random tail to the url to prevent JSON from being loaded from the browser cache
                var response = await dispatcher.DownloadIntoMemory<ResponseText>(url + "?tail=" + Guid.NewGuid(), token, Timeout);
#else
                ResponseText response = await dispatcher.DownloadIntoMemory<ResponseText>(url, token, Timeout);
#endif
                return ParseResponse(response.Text);
            }
            catch (CustomException error)
            {
                var message = error.Message;
                var failureType = error.FailureType;

                if (failureType == FailureType.MetadataParseError)
                {
                    failureType = error.FailureType;
                }
                else if (failureType != FailureType.NoInternetConnection)
                {
                    failureType = FailureType.MetadataDownloadError;
                    message = $"Failed to download metadata into memory. {error}";
                }

                SDKLogger.Log(TAG, message);
                throw new CustomException(failureType, message);
            }
        }

        private void SaveMetadataToFile(AvatarContext avatarContext)
        {
            avatarContext.Metadata.SaveToFile(avatarContext.AvatarUri.Guid, avatarContext.AvatarUri.LocalMetadataPath);
        }

        /// <summary>
        /// This method checks if the avatar model has been updated.
        /// </summary>
        /// <param name="context">The avatar context with all the data required to check if the avatar needs to be updated.</param>
        /// <returns>A <c>bool</c> indicating if the avatar has been updated.</returns>
        /// r=
        /// <remarks>
        /// It is used to determine whether an avatar needs to be downloaded again or can instead be loaded from the
        /// locally stored file.
        /// </remarks>
        private static bool IsUpdateRequired(AvatarContext context)
        {
            if (!context.AvatarCachingEnabled)
            {
                return true;
            }
            AvatarMetadata previousMetadata = AvatarMetadata.LoadFromFile(context.AvatarUri.LocalMetadataPath);
            return AvatarMetadata.IsUpdated(context.Metadata, previousMetadata) || ShouldUpdateAvatarFile(context);
        }

        private static bool ShouldUpdateAvatarFile(AvatarContext context)
        {
            if (!File.Exists(context.AvatarUri.LocalModelPath))
            {
                return true;
            }
            var avatarFileUpdateTime = File.GetLastWriteTimeUtc(context.AvatarUri.LocalModelPath);
            return avatarFileUpdateTime < context.Metadata.UpdatedAt;
        }

        /// <summary>
        /// This method deserializes the response and parses it as an <see cref="AvatarMetadata" /> structure.
        /// </summary>
        /// <param name="response">The response as a json string.</param>
        /// <param name="lastModified">A string representing the date of the last time the metadata was modified.</param>
        /// <returns>The avatar metadata as an <see cref="AvatarMetadata" /> structure.</returns>
        private AvatarMetadata ParseResponse(string response)
        {
            var metadata = JsonConvert.DeserializeObject<AvatarMetadata>(response, new JsonSerializerSettings
            {
                DateFormatString = METADATA_TIME_FORMAT
            });

            if (metadata.BodyType == BodyType.None)
            {
                throw new CustomException(FailureType.MetadataParseError, FAILED_TO_PARSE_METADATA_UNEXPECTED_BODY_TYPE);
            }

            SDKLogger.Log(TAG, $"{metadata.BodyType} metadata loading completed.");
            return metadata;
        }
    }
}

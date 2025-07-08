using System;
using System.Threading.Tasks;
using ReadyPlayerMe.Loader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// The <c>AvatarObjectLoader</c> is responsible for loading the avatar from a url and spawning it as a GameObject in
    /// the scene.
    /// </summary>
    public class AvatarObjectLoader
    {
        private const string TAG = nameof(AvatarObjectLoader);

        private readonly bool avatarCachingEnabled;

        /// Scriptable Object Avatar API request parameters configuration
        public AvatarConfig AvatarConfig;

        /// Importer to use to import glTF
        public IImporter Importer;

        /// Custom defer agent which decides how glTF will be imported 
        public GLTFDeferAgent GLTFDeferAgent;

        private string avatarUrl;
        private OperationExecutor<AvatarContext> executor;
        private float startTime;

        /// <summary>
        /// This class constructor is used to any required fields.
        /// </summary>
        /// <param name="useDefaultGLTFDeferAgent">Use default defer agent</param>
        public AvatarObjectLoader(bool useDefaultGLTFDeferAgent = true)
        {
            AvatarLoaderSettings loaderSettings = AvatarLoaderSettings.LoadSettings();
            avatarCachingEnabled = loaderSettings && loaderSettings.AvatarCachingEnabled;
            AvatarConfig = loaderSettings.AvatarConfig != null ? loaderSettings.AvatarConfig : null;

            if (!useDefaultGLTFDeferAgent && loaderSettings.GLTFDeferAgent != null)
            {
                GLTFDeferAgent = Object.Instantiate(loaderSettings.GLTFDeferAgent).GetComponent<GLTFDeferAgent>();
            }
        }

        /// Set the timeout for download requests
        public int Timeout { get; set; } = 20;

        /// Called upon avatar loader failure.
        public event EventHandler<FailureEventArgs> OnFailed;

        /// Called upon avatar loader progress change.
        public event EventHandler<ProgressChangeEventArgs> OnProgressChanged;

        /// Called upon avatar loader success.
        public event EventHandler<CompletionEventArgs> OnCompleted;

        public event EventHandler<IOperation<AvatarContext>> OperationCompleted;

        /// <summary>
        /// Load avatar from a URL.
        /// </summary>
        /// <param name="url">The URL to the avatars .glb file.</param>
        public void LoadAvatar(string url)
        {
            startTime = Time.timeSinceLevelLoad;
            SDKLogger.Log(TAG, $"Started loading avatar with config {(AvatarConfig ? AvatarConfig.name : "None")} from URL {url}");
            avatarUrl = url;
            Load(url);
        }

        /// <summary>
        /// Load avatar asynchronously from a URL and return the result as eventArgs.
        /// </summary>
        /// <param name="url">The URL to the avatars .glb file.</param>
        public async Task<EventArgs> LoadAvatarAsync(string url)
        {
            EventArgs eventArgs = null;
            var isCompleted = false;
            OnCompleted += (sender, args) =>
            {
                eventArgs = args;
                isCompleted = true;
            };
            OnFailed += (sender, args) =>
            {
                eventArgs = args;
                isCompleted = true;
            };

            startTime = Time.timeSinceLevelLoad;
            SDKLogger.Log(TAG, $"Started loading avatar with config {(AvatarConfig ? AvatarConfig.name : "None")} from URL {url}");
            avatarUrl = url;
            Load(url);

            while (!isCompleted)
            {
                await Task.Yield();
            }
            return eventArgs;
        }

        /// <summary>
        /// Cancel avatar loading
        /// </summary>
        public void Cancel()
        {
            executor.Cancel();
        }

        /// <summary>
        /// Runs through the process of loading the avatar and creating a game object via the <c>OperationExecutor</c>.
        /// </summary>
        /// <param name="url">The URL to the avatars .glb file.</param>
        private async void Load(string url)
        {
            var context = new AvatarContext();
            context.Url = url;
            context.AvatarCachingEnabled = avatarCachingEnabled;
            context.AvatarConfig = AvatarConfig;
            context.ParametersHash = AvatarCache.GetAvatarConfigurationHash(AvatarConfig);

            executor = new OperationExecutor<AvatarContext>(new IOperation<AvatarContext>[]
            {
                new UrlProcessor(),
                new MetadataDownloader(),
                new AvatarDownloader(),
                Importer ?? new GltFastAvatarImporter(GLTFDeferAgent),
                new AvatarProcessor()
            });
            executor.ProgressChanged += ProgressChanged;
            executor.Timeout = Timeout;
            executor.OperationCompleted += op => OperationCompleted?.Invoke(this, op);

            ProgressChanged(0, nameof(AvatarObjectLoader));
            try
            {
                context = await executor.Execute(context);
            }
            catch (CustomException exception)
            {
                Failed(executor.IsCancelled ? FailureType.OperationCancelled : exception.FailureType, exception.Message);
                return;
            }
            catch (Exception e)
            {
                Failed(FailureType.Unknown, e.Message);
                return;
            }

            var avatar = (GameObject) context.Data;
            avatar.SetActive(true);

            var avatarData = avatar.AddComponent<AvatarData>();
            avatarData.AvatarId = avatar.name;
            avatarData.AvatarMetadata = context.Metadata;

            OnCompleted?.Invoke(this, new CompletionEventArgs
            {
                Avatar = avatar,
                Url = context.Url,
                Metadata = context.Metadata
            });

            SDKLogger.Log(TAG, $"Avatar loaded in {Time.timeSinceLevelLoad - startTime:F2} seconds.");
        }

        /// <summary>
        /// This function is called everytime the progress changes on a given IOperation.
        /// </summary>
        /// <param name="progress">The progress of the current operation.</param>
        /// <param name="type">The type of operation that it has changed to.</param>
        private void ProgressChanged(float progress, string type)
        {
            OnProgressChanged?.Invoke(this, new ProgressChangeEventArgs
            {
                Operation = type,
                Url = avatarUrl,
                Progress = progress
            });
        }

        /// <summary>
        /// This function is called if the async <c>Load()</c> function fails either due to error or cancellation.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        private void Failed(FailureType type, string message)
        {
            OnFailed?.Invoke(this, new FailureEventArgs
            {
                Type = type,
                Url = avatarUrl,
                Message = message
            });
            SDKLogger.Log(TAG, $"Failed to load avatar. Error type {type}. URL {avatarUrl}. Message {message}");
        }
    }
}

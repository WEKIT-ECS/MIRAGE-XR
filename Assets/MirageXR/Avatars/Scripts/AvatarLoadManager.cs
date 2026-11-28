using Cysharp.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using LearningExperienceEngine.NewDataModel;
using MirageXR.FileCache;

namespace MirageXR
{
    public class AvatarLoadManager : MonoBehaviour, IThumbnailProvider
    {
        private string AvatarBaseEndpoint
        {
            get
            {
                var config = RootObject.Instance?.LEE?.ConfigManager;
                var address = config?.GetNetworkServerAddress();
                if (string.IsNullOrEmpty(address))
                {
                    address = "https://repository.wekit-ecs.com";
                }
                if (address.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && address.Contains("repository.wekit-ecs.com"))
                {
                    address = "https://" + address.Substring("http://".Length);
                }
                var port = config?.GetNetworkServerPort();
                var baseUri = string.IsNullOrEmpty(port) ? address.TrimEnd('/') : $"{address.TrimEnd('/')}:{port}";
                return $"{baseUri}/avatar/";
            }
        }

        private const string ModelFileExtension = ".glb";
        private const string ThumbnailFileExtension = ".img";

        private LearningExperienceEngine.IAuthorizationManager _authorizationManager;
        private IFileCacheService _fileCacheService;

        private string AvatarListEndpoint => AvatarBaseEndpoint + "list";

        private string AvatarThumbnailEndpoint => AvatarBaseEndpoint + "thumbnail/get";

        private string AvatarModelEndpoint => AvatarBaseEndpoint + "get";

        public void Initialize(LearningExperienceEngine.IAuthorizationManager authorizationManager, IFileCacheService fileCacheService)
        {
            _authorizationManager = authorizationManager;
            _fileCacheService = fileCacheService;
        }

        public async Task<string[]> GetListOfAvatarsAsync()
        {
            try
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(AvatarListEndpoint))
                {
                    webRequest.certificateHandler = new BypassCertificateHandler();
                    AuthorizationHeaders.Apply(webRequest, _authorizationManager);
                    await webRequest.SendWebRequest();
                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        AvatarListReponse avatars = JsonConvert.DeserializeObject<AvatarListReponse>(webRequest.downloadHandler.text);
                        avatars.Data.Sort();
                        return avatars.Data.ToArray();
                    }
                    else
                    {
                        Debug.LogWarning("Error fetching avatar list: " + webRequest.error);
                        return new string[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Exception fetching avatar list: " + ex.Message);
                return new string[0];
            }
        }

        public async Task<GltfImport> GetGltfModelAsync(string urlOrAvatarName)
        {
            if (string.IsNullOrWhiteSpace(urlOrAvatarName))
            {
                return null;
            }

            string avatarName = AvatarLoadUtils.GetId(urlOrAvatarName);
            if (string.IsNullOrEmpty(avatarName) || avatarName.Equals("DefaultAvatar", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (_fileCacheService == null)
            {
                Debug.LogError("[AvatarLoadManager] Not initialized, cannot load avatar models.", this);
                return null;
            }

            string url = AvatarModelEndpoint + "?avatar_name=" + avatarName;

            try
            {
                var request = new CacheRequest(url, AuthorizationHeaders.Create(_authorizationManager), ModelFileExtension);
                GltfImport gltf = await _fileCacheService.AvatarModels.LoadAsync(request, LoadGltfFileAsync);
                if (gltf == null)
                {
                    Debug.LogWarning($"[AvatarLoadManager] Could not load avatar model '{avatarName}' from {url}");
                }

                return gltf;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AvatarLoadManager] Exception loading avatar model '{avatarName}': {ex.Message}");
                return null;
            }
        }

        public async Task<GameObject> CreateGameObjectAsync(string avatarName, Transform parent = null)
        {
            GltfImport gltf = await GetGltfModelAsync(avatarName);
            if (gltf == null)
            {
                return null;
            }

            GameObject instance = new GameObject("Avatar - " + avatarName);
            instance.transform.parent = parent;

            bool success = await gltf.InstantiateMainSceneAsync(instance.transform);
            if (success)
            {
                // change the instance name so that the RPM/avatar scripts still work
                var wolfRenderer = instance.transform.Find("Armature/Wolf3D_Avatar");
                if (wolfRenderer != null)
                {
                    wolfRenderer.name = "Renderer_Avatar";
                }
                return instance;
            }
            else
            {
                Destroy(instance);
                Debug.LogError("Error instantiating the GLTF model");
                return null;
            }
        }

        /// <inheritdoc/>
        public async UniTask<Texture2D> GetThumbnailAsync(string avatarName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(avatarName)) return null;

            string cleanName = AvatarLoadUtils.GetId(avatarName);
            if (string.IsNullOrEmpty(cleanName) || cleanName.Equals("DefaultAvatar", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (_fileCacheService == null)
            {
                Debug.LogError("[AvatarLoadManager] Not initialized, cannot load thumbnails.", this);
                return null;
            }

            try
            {
                string thumbnailUrl = AvatarThumbnailEndpoint + "?avatar_name=" + cleanName;
                var request = new CacheRequest(thumbnailUrl, AuthorizationHeaders.Create(_authorizationManager), ThumbnailFileExtension);
                Texture2D thumbnail = await CachedTextureLoader.LoadAsync(_fileCacheService.Thumbnails, request, cancellationToken);
                if (thumbnail == null)
                {
                    Debug.LogWarning($"[AvatarLoadManager] Could not load thumbnail for '{cleanName}'");
                }

                return thumbnail;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarLoadManager] Exception loading thumbnail for '{cleanName}': " + ex.Message);
                return null;
            }
        }

        private static async Task<GltfImport> LoadGltfFileAsync(string path, CancellationToken cancellationToken)
        {
            var gltf = new GltfImport();
            bool success = false;
            try
            {
                success = await gltf.LoadFile(path, null, null, cancellationToken);
                return success ? gltf : null;
            }
            finally
            {
                if (!success)
                {
                    gltf.Dispose();
                }
            }
        }
    }
}

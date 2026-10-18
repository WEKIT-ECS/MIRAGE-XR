using Cysharp.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    public class AvatarLoadManager : MonoBehaviour, IThumbnailProvider
    {
        private const string avatarBaseEndpoint = "http://repository.wekit-ecs.com:8001/avatar/";

        private LearningExperienceEngine.IAuthorizationManager _authorizationManager;

        private Dictionary<string, Texture2D> thumbnailCache = new Dictionary<string, Texture2D>();

        private static readonly SemaphoreSlim _networkLock = new SemaphoreSlim(1, 1);

        private string AvatarListEndpoint
        {
            get => avatarBaseEndpoint + "list";
        }

        private string AvatarThumbnailEndpoint
        {
            get => avatarBaseEndpoint + "thumbnail/get";
        }

        private string AvatarModelEndpoint
        {
            get => avatarBaseEndpoint + "get";
        }

        public void Initialize(LearningExperienceEngine.IAuthorizationManager authorizationManager)
        {
            _authorizationManager = authorizationManager;
        }

        public async Task<string[]> GetListOfAvatarsAsync()
        {
            try
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(AvatarListEndpoint))
                {
                    if (_authorizationManager != null && !string.IsNullOrEmpty(_authorizationManager.AccessToken))
                    {
                        webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                    }
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

            string url = AvatarModelEndpoint + "?avatar_name=" + avatarName;

            try
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    if (_authorizationManager != null && !string.IsNullOrEmpty(_authorizationManager.AccessToken))
                    {
                        webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                    }

                    await webRequest.SendWebRequest();
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"[AvatarLoadManager] Error downloading avatar model for '{avatarName}' from {url}: {webRequest.error}");
                        return null;
                    }
                    byte[] gltfData = webRequest.downloadHandler.data;
                    if (gltfData == null || gltfData.Length == 0)
                    {
                        Debug.LogWarning($"[AvatarLoadManager] Downloaded avatar data for '{avatarName}' is empty.");
                        return null;
                    }

                    GltfImport gltf = new GltfImport();
                    bool success = await gltf.Load(gltfData);

                    if (success)
                    {
                        return gltf;
                    }
                    else
                    {
                        Debug.LogError($"[AvatarLoadManager] Could not load downloaded bytes as GLTF model for '{avatarName}'", this);
                        return null;
                    }
                }
            }
            catch (UnityWebRequestException ex)
            {
                Debug.LogWarning($"[AvatarLoadManager] Failed to fetch avatar model '{avatarName}' from {url}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AvatarLoadManager] Exception downloading avatar model '{avatarName}': {ex.Message}");
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

        public async UniTask<Texture2D> GetThumbnailAsync(string avatarName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(avatarName)) return null;

            string cleanName = AvatarLoadUtils.GetId(avatarName);
            if (string.IsNullOrEmpty(cleanName) || cleanName.Equals("DefaultAvatar", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (thumbnailCache.TryGetValue(cleanName, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            await _networkLock.WaitAsync(cancellationToken);

            try
            {
                // re-check if the thumbnail was loaded in the meantime
                if (thumbnailCache.TryGetValue(cleanName, out Texture2D doubleCheck))
                {
                    return doubleCheck;
                }

                string thumbnailUrl = AvatarThumbnailEndpoint + "?avatar_name=" + cleanName;
                using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(thumbnailUrl))
                {
                    if (_authorizationManager != null && !string.IsNullOrEmpty(_authorizationManager.AccessToken))
                    {
                        webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                    }
                    await webRequest.SendWebRequest().WithCancellation(cancellationToken);

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D thumbnail = DownloadHandlerTexture.GetContent(webRequest);
                        thumbnailCache[cleanName] = thumbnail;
                        return thumbnail;
                    }
                    else
                    {
                        Debug.LogWarning($"[AvatarLoadManager] Error fetching thumbnail for '{cleanName}': " + webRequest.error);
                        return null;
                    }
                }
            }
            catch (UnityWebRequestException ex)
            {
                Debug.LogWarning($"[AvatarLoadManager] Failed to fetch thumbnail for '{cleanName}': " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarLoadManager] Exception fetching thumbnail for '{cleanName}': " + ex.Message);
                return null;
            }
            finally
            {
                // release network lock
                _networkLock.Release();
            }
        }
    }
}

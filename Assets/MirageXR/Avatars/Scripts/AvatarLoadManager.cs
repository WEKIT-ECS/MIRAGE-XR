using Cysharp.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json;
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
            using (UnityWebRequest webRequest = UnityWebRequest.Get(AvatarListEndpoint))
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                await webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    AvatarListReponse avatars = JsonConvert.DeserializeObject<AvatarListReponse>(webRequest.downloadHandler.text);
                    avatars.Data.Sort();
                    return avatars.Data.ToArray();
                }
                else
                {
                    Debug.LogError("Error fetching avatar list: " + webRequest.error);
                    return new string[0];
                }
            }
        }

        public async Task<GltfImport> GetGltfModelAsync(string avatarName)
        {
            string url = AvatarModelEndpoint + "?avatar_name=" + avatarName;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error downloading avatar model for {avatarName}", this);
                    return null;
                }
                byte[] gltfData = webRequest.downloadHandler.data;

                GltfImport gltf = new GltfImport();

                bool success = await gltf.Load(gltfData);

                if (success)
                {
                    return gltf;
                }
                else
                {
                    Debug.LogError($"Could not load downloaded bytes as GLTF model for {avatarName}", this);
                    return null;
                }
            }
        }

        public async Task<GameObject> CreateGameObjectAsync(string avatarName, Transform parent = null)
        {
            GltfImport gltf = await GetGltfModelAsync(avatarName);
            if (gltf == null)
            {
                Debug.LogError("Call to GetGltfModel did not return model data");
                return null;
            }

            GameObject instance = new GameObject("Avatar - " + avatarName);
            instance.transform.parent = parent;

            bool success = await gltf.InstantiateMainSceneAsync(instance.transform);
            if (success)
            {
                // change the instance name so that the RPM scripts still work
                instance.transform.Find("Armature/Wolf3D_Avatar").name = "Renderer_Avatar";
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
            if (thumbnailCache.TryGetValue(avatarName, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            await _networkLock.WaitAsync(cancellationToken);

            try
            {
                // re-check if the thumbnail was loaded in the meantime
                if (thumbnailCache.TryGetValue(avatarName, out Texture2D doubleCheck))
                {
                    return doubleCheck;
                }

                string thumbnailUrl = AvatarThumbnailEndpoint + "?avatar_name=" + avatarName;
                using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(thumbnailUrl))
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                    await webRequest.SendWebRequest().WithCancellation(cancellationToken);

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D thumbnail = DownloadHandlerTexture.GetContent(webRequest);
                        thumbnailCache[avatarName] = thumbnail;
                        return thumbnail;
                    }
                    else
                    {
                        Debug.LogError("Error fetching thumbnail: " + webRequest.error);
                        return null;
                    }
                }
            }
            finally
            {
                // release network lock
                _networkLock.Release();
            }
        }
    }
}

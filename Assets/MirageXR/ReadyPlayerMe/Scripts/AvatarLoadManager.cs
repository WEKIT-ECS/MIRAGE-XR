using GLTFast;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    public class AvatarLoadManager : MonoBehaviour
    {
        private const string avatarBaseEndpoint = "http://repository.wekit-ecs.com:8001/avatar/";

        private LearningExperienceEngine.IAuthorizationManager _authorizationManager;

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

        public async Task<Texture2D> GetThumbnailAsync(string avatarName)
        {
            string thumbnailUrl = AvatarThumbnailEndpoint + "?avatar_name=" + avatarName;
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(thumbnailUrl))
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Texture2D thumbnail = DownloadHandlerTexture.GetContent(webRequest);
                    return thumbnail;
                }
                else
                {
                    Debug.LogError("Error fetching thumbnail: " + webRequest.error);
                    return null;
                }
            }
        }
    }
}

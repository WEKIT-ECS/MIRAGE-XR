using GLTFast;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    public class AvatarLoadManager : MonoBehaviour
    {
        private string avatarListUrl;
        private string avatarThumbnailEndpoint;
        private const string avatarThumbnailFiletype = "png";
        private string avatarModelEndpoint;
        private const string avatarModelFiletype = "glb";

        private void Awake()
        {
            avatarListUrl = "file://" + Application.persistentDataPath + "/AvatarMock/avatarList.txt";
            avatarThumbnailEndpoint = "file://" + Application.persistentDataPath + "/AvatarMock/";
            avatarModelEndpoint = "file://" + Application.persistentDataPath + "/AvatarMock/";
        }

        public async Task<string[]> GetListOfAvatarsAsync()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(avatarListUrl))
            {
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string[] avatars = JsonConvert.DeserializeObject<string[]>(webRequest.downloadHandler.text);
                    return avatars;
                }
                else
                {
                    Debug.LogError("Error fetching avatar list: " + webRequest.error);
                    return new string[0];
                }
            }
        }

        public async Task<GltfImport> GetGltfModel(string avatarName)
        {
            string url = avatarModelEndpoint + avatarName + "." + avatarModelFiletype;

            GltfImport gltf = new GltfImport();

            bool success = await gltf.Load(url);

            if (success)
            {
                return gltf;
            }
            else
            {
                return null;
            }
        }

        public async Task<GameObject> LoadModel(string avatarName, Transform parent = null)
        {
            GltfImport gltf = await GetGltfModel(avatarName);
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
                return instance;
            }
            else
            {
                Destroy(instance);
                Debug.LogError("Error instantiating the GLTF model");
                return null;
            }
        }

        public async Task<Texture2D> GetThumbnail(string avatarName)
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(avatarThumbnailEndpoint + avatarName + "." + avatarThumbnailFiletype))
            {
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

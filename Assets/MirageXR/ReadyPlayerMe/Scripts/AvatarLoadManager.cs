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
        private string avatarThumbnailFiletype = "png";

        private void Awake()
        {
            avatarListUrl = "file://" + Application.persistentDataPath + "/AvatarMock/avatarList.txt";
            avatarThumbnailEndpoint = "file://" + Application.persistentDataPath + "/AvatarMock/";
        }

        private async void Start()
        {
            Texture2D tex = await GetThumbnail("DefaultAvatar");
            GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            testObj.GetComponent<Renderer>().material.mainTexture = tex;
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

        public async Task<Texture2D> GetThumbnail(string avatarName)
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(avatarThumbnailEndpoint + avatarName + "." + avatarThumbnailFiletype))
            {
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

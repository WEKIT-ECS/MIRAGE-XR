using UnityEngine;
using UnityEngine.Networking;

namespace ReadyPlayerMe.Core
{
    public class ResponseTexture : IResponse
    {
        public Texture2D Texture;

        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public long ResponseCode { get; set; }

        public void Parse(UnityWebRequest request)
        {
            if (IsSuccess)
            {
                Texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
            }
        }
    }
}

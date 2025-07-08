using UnityEngine.Networking;

namespace ReadyPlayerMe.Core
{
    public class ResponseData : IResponse
    {
        public byte[] Data;

        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public long ResponseCode { get; set; }

        public void Parse(UnityWebRequest request)
        {
            if (request.downloadHandler is DownloadHandlerFile)
            {
                return;
            }

            Data = request.downloadHandler.data;
        }
    }
}

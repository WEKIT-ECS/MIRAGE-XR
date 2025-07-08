using UnityEngine.Networking;

namespace ReadyPlayerMe.Core
{
    public class ResponseText : IResponse
    {
        public string Text;

        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public long ResponseCode { get; set; }

        public void Parse(UnityWebRequest request)
        {
            if (request.downloadHandler is DownloadHandlerFile)
            {
                return;
            }

            Text = request.downloadHandler.text;
        }
    }
}

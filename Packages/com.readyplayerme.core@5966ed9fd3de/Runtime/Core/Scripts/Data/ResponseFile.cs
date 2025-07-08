using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace ReadyPlayerMe.Core
{
    public class ResponseFile : IResponse
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public long ResponseCode { get; set; }

        public byte[] Data { get; private set; }

        private ulong length;

        public void Parse(UnityWebRequest request)
        {
            length = request.downloadedBytes;
        }

        public async Task ReadFile(string path, CancellationToken token)
        {
            var byteLength = (long) length;
            var info = new FileInfo(path);

            while (info.Length != byteLength && !token.IsCancellationRequested)
            {
                info.Refresh();
                await Task.Yield();
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            // Reading file since can't access raw bytes from downloadHandler
            Data = File.ReadAllBytes(path);
        }
    }
}

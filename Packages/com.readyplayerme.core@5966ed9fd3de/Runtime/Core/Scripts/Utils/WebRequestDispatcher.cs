using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ReadyPlayerMe.Core
{
    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    public class WebRequestDispatcher
    {
        private const string REQUEST_CANCEL_ERROR = "Request was cancelled";
        public int Timeout = 240;

        public Action<float> ProgressChanged;

        public async Task<T> SendRequest<T>(
            string url,
            HttpMethod httpMethod,
            IDictionary<string, string> headers = null,
            string payload = null,
            DownloadHandler downloadHandler = default,
            CancellationToken ctx = new CancellationToken()) where T : IResponse, new()
        {
            using var request = new UnityWebRequest();
            request.timeout = Timeout;
            request.url = url;
            request.method = httpMethod.ToString();

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            downloadHandler ??= new DownloadHandlerBuffer();

            request.downloadHandler = downloadHandler;

            if (!string.IsNullOrEmpty(payload))
            {
                var bytes = Encoding.UTF8.GetBytes(payload);
                request.uploadHandler = new UploadHandlerRaw(bytes);
            }

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone && !ctx.IsCancellationRequested)
            {
                await Task.Yield();
                ProgressChanged?.Invoke(request.downloadProgress);
            }

            var response = new T();
            response.ResponseCode = request.responseCode;

            if (ctx.IsCancellationRequested)
            {
                request.Abort();
                response.Error = REQUEST_CANCEL_ERROR;
                response.Parse(request);
                return response;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                response.Error = request.error;
                response.Parse(request);

                if (!(request.downloadHandler is DownloadHandlerBuffer))
                {
                    return response;
                }
                Debug.Log(request.downloadHandler.text + "\n" + request.method.ToUpper() + " " + url);
                return response;
            }

            response.IsSuccess = true;
            response.Parse(request);
            return response;
        }
    }
}

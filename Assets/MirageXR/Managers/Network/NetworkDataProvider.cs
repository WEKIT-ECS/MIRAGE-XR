using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.DTOs;
using Newtonsoft.Json;

namespace MirageXR.NewDataModel
{
    public class NetworkDataProvider : INetworkDataProvider
    {
        private const string ServerURL = "http://91.107.198.4:8000";
        private const string DevelopServerURL = "http://91.107.198.4:8001";

        private static string Token => LearningExperienceEngine.LearningExperienceEngine.Instance.authManager.AccessToken;

        public async UniTask<Response<List<Activity>>> GetActivitiesAsync(CancellationToken cancellationToken = default)
        {
            const string api = "activity/";

            var url = $"{ServerURL}/{api}";
            return await Network.RequestAsync<List<Activity>>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Response<List<ActivityResponse>>> GetActivitiesAsyncTemp(CancellationToken cancellationToken = default)
        {
            const string api = "activity/";

            var url = $"{ServerURL}/{api}";
            return await Network.RequestAsync<List<ActivityResponse>>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Response<string>> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, CancellationToken cancellationToken = default)
        {
            const string api = "assets/get_hash";

            var url = $"{ServerURL}/{api}?activityId={activityId}&contentId={contentId}&fileId={fileId}";
            return await Network.RequestAsync<string>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Response<Activity>> GetActivityAsync(Guid activityId, CancellationToken cancellationToken = default)
        {
            const string api = "activity/get";

            var url = $"{ServerURL}/{api}?activity_id={activityId}";
            return await Network.RequestAsync<Activity>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> UploadActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            const string api = "activity/post";
            const string mediaType = "application/json";

            using var content = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, mediaType);
            var url = $"{ServerURL}/{api}";
            return await Network.RequestAsync<Activity>(url, content, token: Token, type: Network.RequestType.Post, cancellationToken: cancellationToken);
        }

        public async UniTask<Response<string>> UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            const string api = "activity/put";
            const string mediaType = "application/json";

            using var content = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, mediaType);
            var url = $"{ServerURL}/{api}?activity_id={activity.Id}";
            return await Network.RequestAsync<string>(url, content, token: Token, type: Network.RequestType.Put, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> DeleteActivityAsync(Guid activityId, CancellationToken cancellationToken = default)
        {
            const string api = "activity/delete";

            var url = $"{ServerURL}/{api}?activity_id={activityId}";
            return await Network.RequestAsync<string>(url, token: Token, type: Network.RequestType.Delete, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> UploadAssetAsync(Guid activityId, Guid fileId, string filePath, CancellationToken cancellationToken = default)
        {
            const string api = "asset/post";

            var url = $"{DevelopServerURL}/{api}?activity_id={activityId}&file_id={fileId}";

            var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);

            using var content = new ByteArrayContent(bytes);
            using var form = new MultipartFormDataContent();
            form.Add(content, "file", Path.GetFileName(filePath));
            return await Network.RequestAsync<string>(url, form, Token, type: Network.RequestType.Post, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> UpdateAssetAsync(Guid activityId, Guid fileId, string filePath, CancellationToken cancellationToken = default)
        {
            const string api = "asset/put";

            var url = $"{DevelopServerURL}/{api}?activity_id={activityId}&file_id={fileId}";

            var bytes =await File.ReadAllBytesAsync(filePath, cancellationToken);

            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            using var form = new MultipartFormDataContent();
            form.Add(content, "file", Path.GetFileName(filePath));
            form.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            return await Network.RequestAsync<string>(url, form, Token, type: Network.RequestType.Put, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> GetAssetAsync(Stream stream, Guid activityId, Guid fileModelId, CancellationToken cancellationToken = default)
        {
            const string api = "asset/get";

            var url = $"{DevelopServerURL}/{api}?activity_id={activityId}&file_id={fileModelId}";
            return await Network.RequestAsync<string>(stream, url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> GetAssetAsync(string filePath, Guid activityId, Guid fileModelId, CancellationToken cancellationToken = default)
        {
            const string api = "asset/get";

            var url = $"{DevelopServerURL}/{api}?activity_id={activityId}&file_id={fileModelId}";
            return await Network.RequestAsync<string>(filePath, url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }
    }
}
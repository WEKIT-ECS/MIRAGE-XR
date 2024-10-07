using System;
using System.Collections.Generic;
using System.Net.Http;
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

        private static string Token => LearningExperienceEngine.LearningExperienceEngine.Instance.authManager.AccessToken;

        public async UniTask<Response<List<Activity>>> GetActivitiesAsync(CancellationToken cancellationToken = default)
        {
            const string api = "activity/";

            var url = $"{ServerURL}/{api}";
            var data = await Network.RequestAsync<List<Activity>>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
            return data;
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

            var content = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, mediaType);
            var url = $"{ServerURL}/{api}";
            return await Network.RequestAsync<Activity>(url, content, token: Token, type: Network.RequestType.Post, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> GetAssetAsync(string activityId, string fileId, CancellationToken cancellationToken = default)
        {
            const string api = "assets/post";
            const string mediaType = "application/json";

            var url = $"{ServerURL}/{api}";

            return await Network.RequestAsync<Response>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> UploadAssetAsync(RequestUploadAsset requestUploadAsset, CancellationToken cancellationToken = default)
        {
            const string api = "assets/post";
            const string mediaType = "application/json";

            var url = $"{ServerURL}/{api}";

            var content = new StringContent(JsonConvert.SerializeObject(requestUploadAsset), Encoding.UTF8, mediaType);
            return await Network.RequestAsync<Response>(url, content, token: Token, type: Network.RequestType.Post, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> UpdateAssetAsync(RequestUploadAsset requestUploadAsset, CancellationToken cancellationToken = default)
        {
            const string api = "assets/post";
            const string mediaType = "application/json";

            var url = $"{ServerURL}/{api}";

            var content = new StringContent(JsonConvert.SerializeObject(requestUploadAsset), Encoding.UTF8, mediaType);
            return await Network.RequestAsync<Response>(url, content, token: Token, type: Network.RequestType.Put, cancellationToken: cancellationToken);
        }

        public async UniTask<Response> DownloadAssetAsync(Guid activityId, Guid fileModelId, CancellationToken cancellationToken = default)
        {
            const string api = "assets/get";

            var url = $"{ServerURL}/{api}?activity_id={activityId}&fileName={fileModelId}";
            return await Network.RequestAsync<Activity>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }
    }
}
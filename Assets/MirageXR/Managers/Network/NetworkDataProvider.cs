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

        public async UniTask<List<ActivityResponse>> GetActivitiesAsync(CancellationToken cancellationToken = default)
        {
            const string api = "activity/";

            var url = $"{ServerURL}/{api}";
            return await Network.RequestAsync<List<ActivityResponse>>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<string> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, CancellationToken cancellationToken = default)
        {
            const string api = "assets/get_hash";

            var url = $"{ServerURL}/{api}?activityId={activityId}&contentId={contentId}&fileId={fileId}";
            return await Network.RequestAsync<string>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Activity> GetActivityAsync(Guid activityId, CancellationToken cancellationToken = default)
        {
            const string api = "activity/get";

            var url = $"{ServerURL}/{api}?activity_id={activityId}";
            return await Network.RequestAsync<Activity>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Activity> UploadActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            const string api = "activity/post";
            const string mediaType = "application/json";

            var content = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, mediaType);
            var url = $"{ServerURL}/{api}";
            return await Network.RequestAsync<Activity>(url, content, token: Token, type: Network.RequestType.Post, cancellationToken: cancellationToken);
        }

        public async UniTask DownloadContentAsync(Guid activityId, Guid contentId, Guid fileModelId, CancellationToken cancellationToken = default)
        {
            const string api = "assets/get";

            var url = $"{ServerURL}/{api}?activity_id={activityId}&contentId={contentId}&fileId={fileModelId}";
            await Network.RequestAsync<Activity>(url, token: Token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }
    }
}
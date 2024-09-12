using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.DTOs;

namespace MirageXR.NewDataModel
{
    public class NetworkDataProvider : INetworkDataProvider
    {
        private const string ServerURL = "http://91.107.198.4:8000";

        public async UniTask<List<ActivityResponse>> GetActivitiesAsync(string token, CancellationToken cancellationToken = default)
        {
            const string api = "activity/";

            var url = $"{ServerURL}/{api}";
            return await Network.RequestAsync<List<ActivityResponse>>(url, token: token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<string> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, string token, CancellationToken cancellationToken = default)
        {
            const string api = "activity/hash/get";

            var url = $"{ServerURL}/{api}?activityId={activityId}&contentId={contentId}&fileId={fileId}";
            return await Network.RequestAsync<string>(url, token: token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Activity> GetActivityAsync(Guid activityId, string token, CancellationToken cancellationToken = default)
        {
            const string api = "activity/get";

            var url = $"{ServerURL}/{api}?activity_id={activityId}";
            return await Network.RequestAsync<Activity>(url, token: token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }
    }
}
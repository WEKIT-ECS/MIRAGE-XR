using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;

namespace MirageXR.NewDataModel
{
    public class NetworkDataProvider : INetworkDataProvider
    {
        private const string ServerURL = "base url";

        public async UniTask<Response<List<Activity>>> GetActivitiesAsync(string token, CancellationToken cancellationToken = default)
        {
            const string api = "api/activities";

            var url = $"{ServerURL}/{api}";
            return await Network.RequestAsync<List<Activity>>(url, token: token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }

        public async UniTask<Response<string>> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, string token, CancellationToken cancellationToken = default)
        {
            const string api = "api/content/hash";

            var url = $"{ServerURL}/{api}?activityId={activityId}&contentId={contentId}&fileId={fileId}";
            return await Network.RequestAsync<string>(url, token: token, type: Network.RequestType.Get, cancellationToken: cancellationToken);
        }
    }
}
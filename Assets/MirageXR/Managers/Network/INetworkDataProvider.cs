using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;

namespace MirageXR.NewDataModel
{
    public interface INetworkDataProvider
    {
        UniTask<Response<List<Activity>>> GetActivitiesAsync(string token, CancellationToken cancellationToken = default);
        UniTask<Response<string>> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, string token, CancellationToken cancellationToken = default);
    }
}
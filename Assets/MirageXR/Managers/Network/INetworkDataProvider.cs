using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.DTOs;

namespace MirageXR.NewDataModel
{
    public interface INetworkDataProvider
    {
        UniTask<List<ActivityResponse>> GetActivitiesAsync(string token, CancellationToken cancellationToken = default);
        UniTask<string> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, string token, CancellationToken cancellationToken = default);
        UniTask<Activity> GetActivityAsync(Guid activityId, string token, CancellationToken cancellationToken = default);
    }
}
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
        UniTask<List<ActivityResponse>> GetActivitiesAsync(CancellationToken cancellationToken = default);
        UniTask<string> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, CancellationToken cancellationToken = default);
        UniTask<Activity> GetActivityAsync(Guid activityId, CancellationToken cancellationToken = default);
        UniTask DownloadContentAsync(Guid activityId, Guid contentId, Guid fileModelId, CancellationToken cancellationToken = default);
    }
}
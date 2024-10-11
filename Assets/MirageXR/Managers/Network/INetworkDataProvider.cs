using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.DTOs;

namespace MirageXR.NewDataModel
{
    public interface INetworkDataProvider
    {
        UniTask<Response<List<Activity>>> GetActivitiesAsync(CancellationToken cancellationToken = default);
        UniTask<Response<List<ActivityResponse>>> GetActivitiesAsyncTemp(CancellationToken cancellationToken = default);
        UniTask<Response<string>> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, CancellationToken cancellationToken = default);
        UniTask<Response<Activity>> GetActivityAsync(Guid activityId, CancellationToken cancellationToken = default);
        UniTask<Response> UploadActivityAsync(Activity activity, CancellationToken cancellationToken = default);
        UniTask<Response<string>> UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default);
        UniTask<Response> UploadAssetAsync(Guid activityId, Guid fileId, string filePath, CancellationToken cancellationToken = default);
        UniTask<Response> UpdateAssetAsync(Guid activityId, Guid fileId, string filePath, CancellationToken cancellationToken = default);
        UniTask<Response> GetAssetAsync(Stream stream, Guid activityId, Guid fileModelId, CancellationToken cancellationToken = default);
        UniTask<Response> DeleteActivityAsync(Guid activityId, CancellationToken cancellationToken = default);
    }
}
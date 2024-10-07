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
        UniTask<Response<List<Activity>>> GetActivitiesAsync(CancellationToken cancellationToken = default);
        UniTask<Response<string>> GetContentHashAsync(Guid activityId, Guid contentId, Guid fileId, CancellationToken cancellationToken = default);
        UniTask<Response<Activity>> GetActivityAsync(Guid activityId, CancellationToken cancellationToken = default);
        UniTask<Response> UploadActivityAsync(Activity activity, CancellationToken cancellationToken = default);
        UniTask<Response> UploadAssetAsync(RequestUploadAsset requestUploadAsset, CancellationToken cancellationToken = default);
        UniTask<Response> UpdateAssetAsync(RequestUploadAsset requestUploadAsset, CancellationToken cancellationToken = default);
        UniTask<Response> DownloadAssetAsync(Guid activityId, Guid fileModelId, CancellationToken cancellationToken = default);
    }
}
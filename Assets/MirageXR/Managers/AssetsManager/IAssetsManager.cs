using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using MirageXR.View;

namespace MirageXR.NewDataModel
{
    public interface IAssetsManager
    {
        UniTask InitializeAsync(INetworkDataProvider networkDataProvider, IActivityManager activityManager);

        UniTask PrepareContent(Guid activityId, Content content);
        FileModel GetDefaultThumbnail();
        UniTask<FileModel> CreateFileAsync(string folderPath, Guid activityId, Guid contentId);
        ContentView GetContentView(ContentType contentType);
        string GetFolderPath(Guid contentId, Guid fileId);
        string GetFolderPath(Guid activityId, Guid contentId, Guid fileId);
        string GetZipPath(Guid activityId, Guid contentId, Guid fileId);
    }
}
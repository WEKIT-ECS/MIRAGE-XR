using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;

namespace MirageXR.NewDataModel
{
    public interface IAssetsManager
    {
        UniTask InitializeAsync(INetworkDataProvider networkDataProvider);

        UniTask PrepareContent(Guid activityId, Content content);
        FileModel GetDefaultThumbnail();
        UniTask<FileModel> CreateFileAsync(string folderPath, Guid activityId, Guid contentId);
        string GetFilePath(Guid activityId, Guid contentId, Guid fileId);
    }
}
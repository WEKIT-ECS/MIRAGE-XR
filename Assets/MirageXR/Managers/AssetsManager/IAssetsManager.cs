using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;

namespace MirageXR.NewDataModel
{
    public interface IAssetsManager
    {
        UniTask InitializeAsync(INetworkDataProvider networkDataProvider);

        UniTask PrepareContent(Content content);
        File GetDefaultThumbnail();
    }
}
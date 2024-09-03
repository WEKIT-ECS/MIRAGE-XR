using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;

namespace MirageXR.NewDataModel
{
    public interface IContentManager
    {
        UniTask LoadContentAsync(Activity activity);
    }
}
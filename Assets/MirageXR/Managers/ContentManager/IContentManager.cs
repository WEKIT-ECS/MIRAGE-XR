using Cysharp.Threading.Tasks;

namespace MirageXR.NewDataModel
{
    public interface IContentManager
    {
        UniTask LoadContentAsync(Activity activity);
    }
}
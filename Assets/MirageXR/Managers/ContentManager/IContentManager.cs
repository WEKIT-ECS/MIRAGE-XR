using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;

namespace MirageXR.NewDataModel
{
    public class UnityEventContents : UnityEvent<List<Content>> {} 

    public interface IContentManager
    {
        event UnityAction<List<Content>> OnContantActivated;

        UniTask InitializeAsync(IAssetsManager assetsManager);
        UniTask LoadContentAsync(Activity activity);
        List<Content> GetContents();
        void AddContent(Content content);
        void Reset();
        void ShowContent(ActivityStep currentStep);
    }
}
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine.Events;

namespace MirageXR.NewDataModel
{
    public class UnityEventContents : UnityEvent<List<Content>> {} 

    public interface IContentManager
    {
        event UnityAction<List<Content>> OnContentActivated;

        UniTask InitializeAsync(IAssetsManager assetsManager, IStepManager stepManager, IActivityManager activityManager);
        UniTask LoadContentAsync(Activity activity);
        List<Content> GetContents();
        void AddContent(Content content);
        void Reset();
        void ShowContent(Guid currentStepId);
        void UpdateContent(Content content);
    }
}
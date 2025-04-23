using LearningExperienceEngine.NewDataModel;
using MirageXR.View;
using UnityEngine;

namespace MirageXR
{
    public interface IViewManager : IManager
    {
        public ActivityView ActivityView { get; }
        public GameObject UiView { get; }
        void Initialize(IActivityManager activityManager, IAssetBundleManager assetBundleManager, PlatformManager platformManager, CollaborationManager collaborationManager);
        Camera GetCamera();
    }
}
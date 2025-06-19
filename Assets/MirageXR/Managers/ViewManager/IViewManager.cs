using LearningExperienceEngine.NewDataModel;
using MirageXR.View;
using UnityEngine;

namespace MirageXR
{
    public interface IViewManager : IManager
    {
        ActivityView ActivityView { get; }
        GameObject UiView { get; }
        BaseCamera BaseCamera { get; }
        Camera Camera { get; }
        void Initialize(IActivityManager activityManager, IAssetBundleManager assetBundleManager, PlatformManager platformManager, CollaborationManager collaborationManager, IXRManager xrManager);
    }
}
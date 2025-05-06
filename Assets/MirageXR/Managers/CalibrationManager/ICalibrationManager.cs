using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using LearningExperienceEngine.NewDataModel;
using UnityEngine;
using UnityEngine.Events;

namespace MirageXR
{
    public interface ICalibrationManager : IManager
    {
        Transform Anchor { get; }
        UnityEvent OnCalibrationStarted { get; }
        UnityEvent OnCalibrationCanceled { get; }
        UnityEvent OnCalibrationFinished { get; }

        event UnityAction<bool> OnCalibrated;

        float AnimationTime { get; }
        bool IsCalibrated { get; }

        UniTask InitializationAsync(IAssetBundleManager assetsManager, IAuthorizationManager authorizationManager);
        Pose GetAnchorPositionAsync();
        void EnableCalibration(bool isRecalibration = false);
        public void ApplyCalibration(bool resetAnchor);
        void SetAnchorPosition(Pose pose, bool resetAnchor);
        void DisableCalibration();
        void StartCalibration();
        void CancelCalibration();
        void FinishCalibration(Pose pose, bool resetAnchor);
    }
}
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using MirageXR.View;
using UnityEngine;

namespace MirageXR
{
    public enum UiType
    {
        Spatial,
        Screen,
    }

    public enum CameraType
    {
        VisionOS,
        OpenXR,
    }

    public interface IAssetBundleManager : IManager
    {
        UniTask InitializeAsync();
        StepView GetStepViewPrefab(bool isNetPrefab = false);
        CalibrationTool GetCalibrationToolPrefab();
        ActivityView GetActivityViewPrefab(bool isNetPrefab = false);
        ContentView GetContentViewPrefab(LearningExperienceEngine.DataModel.ContentType contentType, bool isNetPrefab = false);
        GameObject GetUiView(UiType spatial);
        GameObject GetCamera(CameraType spatial);
    }
}
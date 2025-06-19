using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using MirageXR.View;
using UnityEngine;

namespace MirageXR
{
    [Serializable]
    public enum UiType
    {
        Spatial,
        Screen,
    }

    [Serializable]
    public enum CameraType
    {
        VisionOS,
        OpenXR,
        ARFoundation,
    }

    public interface IAssetBundleManager : IManager
    {
        UniTask InitializeAsync();
        StepView GetStepViewPrefab(bool isNetPrefab = false);
        CalibrationTool GetCalibrationToolPrefab();
        ActivityView GetActivityViewPrefab(bool isNetPrefab = false);
        ContentView GetContentViewPrefab(LearningExperienceEngine.DataModel.ContentType contentType, bool isNetPrefab = false);
        GameObject GetUiView(UiType spatial);
        BaseCamera GetCamera(CameraType spatial);
    }
}
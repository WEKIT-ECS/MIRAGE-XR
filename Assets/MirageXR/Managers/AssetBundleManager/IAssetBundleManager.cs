﻿using Cysharp.Threading.Tasks;
using LearningExperienceEngine.NewDataModel;
using MirageXR.View;

namespace MirageXR
{
    public interface IAssetBundleManager : IManager
    {
        UniTask InitializeAsync();
        StepView GetStepViewPrefab();
        CalibrationTool GetCalibrationToolPrefab();
        ContentView GetContentViewPrefab(LearningExperienceEngine.DataModel.ContentType contentType);
    }
}
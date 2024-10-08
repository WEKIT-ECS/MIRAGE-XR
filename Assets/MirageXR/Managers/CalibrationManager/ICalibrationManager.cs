﻿using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MirageXR.NewDataModel;
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
        float AnimationTime { get; }
        bool IsCalibrated { get; }

        UniTask InitializationAsync(IAssetsManager assetsManager);
        Pose GetAnchorPositionAsync();
        void EnableCalibration(bool isRecalibration = false);
        Task ApplyCalibrationAsync(bool resetAnchor);
        void SetAnchorPosition(Pose pose);
        void DisableCalibration();
        void StartCalibration();
        void CancelCalibration();
        void FinishCalibration(Pose pose);
    }
}
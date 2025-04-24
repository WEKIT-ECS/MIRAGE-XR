using System;
using Cysharp.Threading.Tasks;
using MirageXR.View;
using UnityEngine;

namespace MirageXR
{
    public class AssetBundleManager : IAssetBundleManager
    {
        private const string MirageXRAssetsBundle = "MirageXRAssetsBundle";

        public bool IsInitialized => _isInitialized;

        private AssetsBundle _assetsBundle;
        private bool _isInitialized;

        public UniTask WaitForInitialization()
        {
            return UniTask.WaitUntil(() => _isInitialized);
        }

        public async UniTask InitializeAsync()
        {
            UnityEngine.Debug.Log("Initializing [AssetBundleManager] <--");
            _assetsBundle = await Resources.LoadAsync<AssetsBundle>(MirageXRAssetsBundle) as AssetsBundle;

            if (_assetsBundle == null)
            {
                throw new Exception("MirageXR assets bundle not found");
            }

            _isInitialized = true;
            UnityEngine.Debug.Log("Initializing [AssetBundleManager] -->");
        }

        public StepView GetStepViewPrefab(bool isNetPrefab = false)
        {
            return _assetsBundle.GetStepViewPrefab(isNetPrefab);
        }

        public CalibrationTool GetCalibrationToolPrefab()
        {
            return _assetsBundle.GetCalibrationToolPrefab();
        }

        public ActivityView GetActivityViewPrefab(bool isNetPrefab = false)
        {
            return _assetsBundle.GetActivityViewPrefab(isNetPrefab);
        }

        public ContentView GetContentViewPrefab(LearningExperienceEngine.DataModel.ContentType contentType, bool isNetPrefab = false)
        {
            return _assetsBundle.GetContentViewPrefab(contentType, isNetPrefab);
        }

        public GameObject GetUiView(UiType spatial)
        {
            switch (spatial)
            {
                case UiType.Spatial:
                    return _assetsBundle.GetSpatialUiView();
                case UiType.Screen:
                    return _assetsBundle.GetScreenUiView();
                default:
                    throw new ArgumentOutOfRangeException(nameof(spatial), spatial, null);
            }
        }

        public GameObject GetCamera(CameraType spatial)
        {
            switch (spatial)
            {
                case CameraType.VisionOS:
                    return _assetsBundle.GetVisionCamera();
                case CameraType.OpenXR:
                    return _assetsBundle.GetOpenCamera();
                default:
                    throw new ArgumentOutOfRangeException(nameof(spatial), spatial, null);
            }
        }
    }
}
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
            _assetsBundle = await Resources.LoadAsync<AssetsBundle>(MirageXRAssetsBundle) as AssetsBundle;

            if (_assetsBundle == null)
            {
                throw new Exception("MirageXR assets bundle not found");
            }

            _isInitialized = true;
        }

        public StepView GetStepViewPrefab()
        {
            return _assetsBundle.GetStepViewPrefab();
        }

        public CalibrationTool GetCalibrationToolPrefab()
        {
            return _assetsBundle.GetCalibrationToolPrefab();
        }

        public NetworkObjectSynchronizer GetNetworkObjectPrefab()
        {
            return _assetsBundle.GetNetworkObjectPrefab();
        }

        public ContentView GetContentViewPrefab(LearningExperienceEngine.DataModel.ContentType contentType)
        {
            return _assetsBundle.GetContentViewPrefab(contentType);
        }
    }
}
using System;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using MirageXR.View;
using UnityEngine;

namespace MirageXR
{
    [CreateAssetMenu(fileName = "MirageXRAssetsBundle", menuName = "MirageXR/AssetsBundle", order = 1)]
    public class AssetsBundle : ScriptableObject
    {
        [Serializable]
        public class ContentAssetItem
        {
            public ContentType ContentType;
            public ContentView ContentView;
        }

        [SerializeField] private CalibrationTool _calibrationTool;
        [SerializeField] private StepView _defaultStepView;
        [SerializeField] private ContentView _defaultContentView;
        [SerializeField] private NetworkObjectSynchronizer _networkObject;
        [SerializeField] private ContentAssetItem[] _contentAssets;

        public ContentView GetContentViewPrefab(ContentType contentType)
        {
            foreach (var item in _contentAssets)
            {
                if (item.ContentType == contentType)
                {
                    return item.ContentView;
                }
            }

            AppLog.LogError("No content view found for the content type: " + contentType);
            return _defaultContentView;
        }

        public StepView GetStepViewPrefab()
        {
            return _defaultStepView;
        }

        public NetworkObjectSynchronizer GetNetworkObjectPrefab()
        {
            return _networkObject;
        }

        public CalibrationTool GetCalibrationToolPrefab()
        {
            return _calibrationTool;
        }
    }
}

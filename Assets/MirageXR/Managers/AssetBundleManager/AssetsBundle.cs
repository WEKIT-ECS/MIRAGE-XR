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

        [SerializeField] private GameObject visionCamera;
        [SerializeField] private GameObject openCamera;
        [SerializeField] private GameObject arFoundationCamera;
        [SerializeField] private GameObject spatialUiView;
        [SerializeField] private GameObject screenUiView;
        [SerializeField] private CalibrationTool _calibrationTool;
        [SerializeField] private StepView _defaultStepView;
        [SerializeField] private StepView _defaultNetStepView;
        [SerializeField] private ContentView _defaultContentView;
        [SerializeField] private ContentView _defaultNetContentView;
        [SerializeField] private ActivityView _activityView;
        [SerializeField] private NetworkActivityView _netActivityView;
        [SerializeField] private ContentAssetItem[] _contentAssets;
        [SerializeField] private ContentAssetItem[] _netContentAssets;

        public GameObject GetSpatialUiView()
        {
            return spatialUiView;
        }

        public GameObject GetScreenUiView()
        {
            return screenUiView;
        }

        public GameObject GetVisionCamera()
        {
            return visionCamera;
        }

        public GameObject GetOpenCamera()
        {
            return openCamera;
        }

        public GameObject GetARFoundationCamera()
        {
            return arFoundationCamera;
        }

        public ContentView GetContentViewPrefab(ContentType contentType, bool isNetPrefab = false)
        {
            return isNetPrefab ? GetNetContentViewPrefab(contentType) : GetLocalContentViewPrefab(contentType);
        }

        private ContentView GetLocalContentViewPrefab(ContentType contentType)
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

        private ContentView GetNetContentViewPrefab(ContentType contentType)
        {
            foreach (var item in _netContentAssets)
            {
                if (item.ContentType == contentType)
                {
                    return item.ContentView;
                }
            }

            AppLog.LogError("No content view found for the content type: " + contentType);
            return _defaultNetContentView;
        }

        public StepView GetStepViewPrefab(bool isNetPrefab = false)
        {
            return isNetPrefab ? _defaultNetStepView : _defaultStepView;
        }

        public ActivityView GetActivityViewPrefab(bool isNetPrefab = false)
        {
            return isNetPrefab ? _netActivityView : _activityView;
        }

        public CalibrationTool GetCalibrationToolPrefab()
        {
            return _calibrationTool;
        }
    }
}

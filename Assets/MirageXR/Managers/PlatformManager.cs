using System;
#if UNITY_ANDROID && !UNITY_EDITOR
using i5.Toolkit.Core.VerboseLogging;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace MirageXR
{
    public class PlatformManager : MonoBehaviour
    {
        [Serializable]
        public class LoadObject
        {
            public GameObject prefab;
            public Transform pathToLoad;
        }

        [Tooltip("If you want to test AR in the editor enable this.")]
        [SerializeField] bool forceWorldSpaceUi = false;
        [SerializeField] bool forceToTabletView = false;

        [SerializeField] bool overrideUiType = false;
        [SerializeField] UiType uiType = UiType.Screen;
        [SerializeField] bool overrideCameraType = false;
        [SerializeField] CameraType cameraType = CameraType.OpenXR;

        [SerializeField] GameObject _screenSpaceDebugTool;
        [SerializeField] private LoadObject[] _worldSpaceObjects;
        [SerializeField] private LoadObject[] _screenSpaceObjects;
        [SerializeField] private LoadObject[] _editorObjects;

        private float distanceToCamera = 0.5f;
        private float offsetYFromCamera = 0.5f;

        private bool _worldSpaceUi;
        private string _playerScene = "Player";
        private string _recorderScene = "recorder";
        private string _commonScene = "common";
        private string _activitySelectionScene = "ActivitySelection";
        private Camera _mainCamera;

        public bool WorldSpaceUi => _worldSpaceUi;

        public string PlayerSceneName => _playerScene;

        public string ActivitySelectionScene => _activitySelectionScene;

        public Vector3 GetTaskStationPosition()
        {
            return _mainCamera.transform.TransformPoint(0.25f * Vector3.forward);
        }

        public void Initialization()
        {
            UnityEngine.Debug.Log("Initializing [PlatformManager] <--");
            _mainCamera = Camera.main;

            if (LearningExperienceEngine.UserSettings.developMode)
            {
                if (_screenSpaceDebugTool)
                {
                    Instantiate(_screenSpaceDebugTool);
                }
            }

#if UNITY_EDITOR
            if (_editorObjects != null)
            {
                foreach (var editorObject in _editorObjects)
                {
                    InstantiateObject(editorObject);
                }
            }
#endif

#if UNITY_ANDROID || UNITY_IOS
            _worldSpaceUi = forceWorldSpaceUi;
#else
            _worldSpaceUi = true;
#endif
            if (_worldSpaceUi)
            {
                if (_worldSpaceObjects != null)
                {
                    foreach (var worldSpaceObject in _worldSpaceObjects)
                    {
                        InstantiateObject(worldSpaceObject);
                    }
                }
            }
            else
            {
                if (_screenSpaceObjects != null)
                {
                    foreach (var screenSpaceObject in _screenSpaceObjects)
                    {
                        InstantiateObject(screenSpaceObject);
                    }
                }
            }
            UnityEngine.Debug.Log("Initializing [PlatformManager] -->");
        }

        public UiType GetUiType()
        {
            if (!overrideUiType)
            {
#if VISION_OS// || META_QUEST
                return UiType.Spatial;
#else //UNITY_ANDROID || UNITY_IOS
                return UiType.Screen;
#endif
            }
            
            return uiType;
        }

        public CameraType GetCameraType()
        {
            if (!overrideCameraType)
            {
#if VISION_OS
                return CameraType.VisionOS;
#elif META_QUEST
                return CameraType.OpenXR;
#else //UNITY_ANDROID || UNITY_IOS
                return CameraType.ARFoundation;
#endif
            }

            return cameraType;
        }

        private static void InstantiateObject(LoadObject loadObject)
        {
            if (loadObject.pathToLoad)
            {
                Instantiate(loadObject.prefab, loadObject.pathToLoad);
            }
            else
            {
                Instantiate(loadObject.prefab);
            }
        }

        public static DeviceFormat GetDeviceFormat()
        {
            if (RootObject.Instance.PlatformManager.forceToTabletView) return DeviceFormat.Tablet;
#if UNITY_IOS && !UNITY_EDITOR
            return UnityEngine.iOS.Device.generation.ToString().Contains("iPad") ? DeviceFormat.Tablet : DeviceFormat.Phone;
#elif UNITY_ANDROID && !UNITY_EDITOR
            const float minTabletSize = 7.5f;
            return GetDeviceDiagonalSizeInInches() > minTabletSize ? DeviceFormat.Tablet : DeviceFormat.Phone;
#elif UNITY_WSA && !UNITY_EDITOR
            return DeviceFormat.Unknown;
#else
            return Screen.width > Screen.height ? DeviceFormat.Tablet : DeviceFormat.Phone;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static float GetDeviceDiagonalSizeInInches()
        {
            var screenWidth = Screen.width / Screen.dpi;
            var screenHeight = Screen.height / Screen.dpi;
            var diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            Debug.LogDebug("Getting device inches: " + diagonalInches);

            return diagonalInches;
        }
#endif
    }
}

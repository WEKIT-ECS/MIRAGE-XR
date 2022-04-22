using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

namespace MirageXR
{
    public class PlatformManager : MonoBehaviour
    {
        private const float HORIZONTAL_OFFSET = 0.5f;
        private const float VERTICAL_OFFSET = 0.25f;
        
        [Serializable]
        public class LoadObject
        {
            public GameObject prefab;
            public Transform pathToLoad;
        }
        
        [Tooltip("If you want to test AR in the editor enable this.")]
        [SerializeField] bool forceWorldSpaceUi = true;
        [SerializeField] private LoadObject[] _worldSpaceObjects;
        [SerializeField] private LoadObject[] _screenSpaceObjects;
        
        private float distanceToCamera = 0.5f;
        private float offsetYFromCamera = 0.5f;
        
        private bool _worldSpaceUi;
        private string _playerScene = "Player";
        private string _recorderScene = "recorder";
        private string _commonScene = "common";
        private string _activitySelectionScene = "ActivitySelection";
        private Camera _mainCamera;

        public static PlatformManager Instance { get; private set; }

        public bool WorldSpaceUi => _worldSpaceUi;

        public string PlayerSceneName => _playerScene;
        public string RecorderSceneName => _recorderScene;

        public string CommonSceneName => _commonScene;

        public string ActivitySelectionScene => _activitySelectionScene;

        private void Awake()
        {
            //Singleton
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Platform: " + Application.platform);

            if (Application.platform == RuntimePlatform.WSAPlayerX86 || Application.platform == RuntimePlatform.WSAPlayerARM)
            {

                foreach (var arcm in Resources.FindObjectsOfTypeAll<ARCameraManager>()) Destroy(arcm);      //TODO: remove Resources.FindObjectsOfTypeAll
                foreach (var arm in Resources.FindObjectsOfTypeAll<ARManager>()) Destroy(arm);
                foreach (var ars in Resources.FindObjectsOfTypeAll<ARSession>()) Destroy(ars);
            }


        }

        public Vector3 GetTaskStationPosition()
        {
            return _mainCamera.transform.TransformPoint(Vector3.forward);
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            _mainCamera = Camera.main;

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
    }
}

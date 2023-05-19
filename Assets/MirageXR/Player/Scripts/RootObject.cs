using System;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

namespace MirageXR
{
    public class RootObject : MonoBehaviour
    {
        public static RootObject Instance { get; private set; }

        [SerializeField] private ImageTargetManagerWrapper _imageTargetManager;
        [SerializeField] private CalibrationManager _calibrationManager;
        [SerializeField] private FloorManager _floorManager;
        [SerializeField] private PointCloudManager _pointCloudManager;

        private ActivityManager _activityManager;
        private AugmentationManager _augmentationManager;
        private MoodleManager _moodleManager;
        private EditorSceneService _editorSceneService;
        private WorkplaceManager _workplaceManager;

        public ImageTargetManagerWrapper imageTargetManager => _imageTargetManager;

        public CalibrationManager calibrationManager => _calibrationManager;

        public FloorManager floorManager => _floorManager;

        public ActivityManager activityManager => _activityManager;

        public AugmentationManager augmentationManager => _augmentationManager;

        public MoodleManager moodleManager => _moodleManager;

        public EditorSceneService editorSceneService => _editorSceneService;

        public WorkplaceManager workplaceManager => _workplaceManager;

        private bool _isInitialized;

        public async Task WaitForInitialization()
        {
            while (!_isInitialized)
            {
                await Task.Yield();
            }
        }

        private void Awake()
        {
            if (Instance)
            {
                if (Instance != this)
                {
                    Destroy(gameObject);
                }

                return;
            }

            Instance = this;
            Initialization().AsAsyncVoid();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private async Task Initialization()
        {
            if (_isInitialized)
            {
                return;
            }

            try
            {
                _imageTargetManager ??= new GameObject("ImageTargetManagerWrapper").AddComponent<ImageTargetManagerWrapper>();
                _calibrationManager ??= new GameObject("CalibrationManager").AddComponent<CalibrationManager>();
                _floorManager ??= new GameObject("FloorManager").AddComponent<FloorManager>();
                _pointCloudManager ??= new GameObject("PointCloudManager").AddComponent<PointCloudManager>();

                _activityManager = new ActivityManager();
                _augmentationManager = new AugmentationManager();
                _moodleManager = new MoodleManager();
                _editorSceneService = new EditorSceneService();
                _workplaceManager = new WorkplaceManager();

                await _imageTargetManager.InitializationAsync();
                await _floorManager.InitializationAsync();
                await _calibrationManager.InitializationAsync();
                await _pointCloudManager.InitializationAsync();

                _activityManager.Subscription();

                _isInitialized = true;

                //EventManager.OnPlayerReset += ResetManagers;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
            }
        }

        private void ResetManagers()
        {
            ResetManagersAsync().AsAsyncVoid();
        }

        private async Task ResetManagersAsync()
        {
            await _floorManager.ResetAsync();
            await _pointCloudManager.ResetAsync();
        }

        private void OnDestroy()
        {
            _activityManager.Unsubscribe();
            _floorManager.Unsubscribe();
            _pointCloudManager.Unsubscribe();
            _activityManager.OnDestroy();
        }
    }
}
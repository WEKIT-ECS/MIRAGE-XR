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
        [SerializeField] private FloorManagerWrapper _floorManager;
        [SerializeField] private PointCloudManager _pointCloudManager;
        [SerializeField] private BrandManager _brandManager;
        [SerializeField] private GridManager _gridManager;

        private ActivityManager _activityManager;
        private AugmentationManager _augmentationManager;
        private MoodleManager _moodleManager;
        private EditorSceneService _editorSceneService;
        private WorkplaceManager _workplaceManager;

        public ImageTargetManagerWrapper imageTargetManager => _imageTargetManager;

        public CalibrationManager calibrationManager => _calibrationManager;

        public FloorManagerWrapper floorManager => _floorManager;

        public BrandManager brandManager => _brandManager;

        public GridManager gridManager => _gridManager;

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

        private async Task Initialization() // TODO: create base Manager class
        {
            if (_isInitialized)
            {
                return;
            }

            try
            {
                _brandManager ??= new GameObject("BrandManager").AddComponent<BrandManager>();
                _imageTargetManager ??= new GameObject("ImageTargetManagerWrapper").AddComponent<ImageTargetManagerWrapper>();
                _calibrationManager ??= new GameObject("CalibrationManager").AddComponent<CalibrationManager>();
                _floorManager ??= new GameObject("FloorManagerWrapper").AddComponent<FloorManagerWrapper>();
                _pointCloudManager ??= new GameObject("PointCloudManager").AddComponent<PointCloudManager>();
                _gridManager ??= new GameObject("GridManager").AddComponent<GridManager>();

                _activityManager = new ActivityManager();
                _augmentationManager = new AugmentationManager();
                _moodleManager = new MoodleManager();
                _editorSceneService = new EditorSceneService();
                _workplaceManager = new WorkplaceManager();

                _brandManager.Initialization();
                await _imageTargetManager.InitializationAsync();
                await _floorManager.InitializationAsync();
                await _calibrationManager.InitializationAsync();
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
                await _pointCloudManager.InitializationAsync();
#endif

                _gridManager.Initialization();

                _activityManager.Subscription();

                _isInitialized = true;

                //EventManager.OnClearAll += ResetManagers;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void ResetManagers()
        {
            ResetManagersAsync().AsAsyncVoid();
        }

        private async Task ResetManagersAsync()
        {
            await _floorManager.ResetAsync();
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
            await _pointCloudManager.ResetAsync();
#endif
            await _imageTargetManager.ResetAsync();
        }

        private void OnDestroy()
        {
            _floorManager.Dispose();
            _activityManager.Unsubscribe();
            _pointCloudManager.Unsubscribe();
            _activityManager.OnDestroy();
        }
    }
}
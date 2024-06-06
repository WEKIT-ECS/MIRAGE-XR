using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class RootObject : MonoBehaviour
    {
        public static RootObject Instance { get; private set; }

        [SerializeField] private Camera _baseCamera;
        [SerializeField] private ImageTargetManagerWrapper _imageTargetManager;
        [SerializeField] private CalibrationManager _calibrationManager;
        [SerializeField] private FloorManagerWrapper _floorManager;
        [SerializeField] private PlaneManagerWrapper _planeManager;
        [SerializeField] private PointCloudManager _pointCloudManager;
        [SerializeField] private BrandManager _brandManager;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private CameraCalibrationChecker _cameraCalibrationChecker;
        [SerializeField] private PlatformManager _platformManager;
        [SerializeField] private ExceptionManager _exceptionManager;

        private ActivityManager _activityManager;
        private AugmentationManager _augmentationManager;
        private MoodleManager _moodleManager;
        private EditorSceneService _editorSceneService;
        private WorkplaceManager _workplaceManager;
        private AIManager _aiManager;
        private OpenAIManager _openAIManager;
        private VirtualInstructorManager _virtualInstructorManager; 

        public Camera baseCamera => _baseCamera;

        public ImageTargetManagerWrapper imageTargetManager => _imageTargetManager;

        public CalibrationManager calibrationManager => _calibrationManager;

        public FloorManagerWrapper floorManager => _floorManager;

        public PlaneManagerWrapper planeManager => _planeManager;

        public BrandManager brandManager => _brandManager;

        public GridManager gridManager => _gridManager;

        public ActivityManager activityManager => _activityManager;

        public AugmentationManager augmentationManager => _augmentationManager;

        public MoodleManager moodleManager => _moodleManager;

        public EditorSceneService editorSceneService => _editorSceneService;

        public WorkplaceManager workplaceManager => _workplaceManager;

        public CameraCalibrationChecker cameraCalibrationChecker => _cameraCalibrationChecker;

        public PlatformManager platformManager => _platformManager;

        public ExceptionManager exceptionManager => _exceptionManager;

        public OpenAIManager openAIManager => _openAIManager;
        public AIManager aiManager => _aiManager;
        public VirtualInstructorManager virtualInstructorManager => _virtualInstructorManager;

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
                _baseCamera ??= Camera.main;
                _brandManager ??= new GameObject("BrandManager").AddComponent<BrandManager>();
                _imageTargetManager ??= new GameObject("ImageTargetManagerWrapper").AddComponent<ImageTargetManagerWrapper>();
                _calibrationManager ??= new GameObject("CalibrationManager").AddComponent<CalibrationManager>();
                _floorManager ??= new GameObject("FloorManagerWrapper").AddComponent<FloorManagerWrapper>();
                _pointCloudManager ??= new GameObject("PointCloudManager").AddComponent<PointCloudManager>();
                _gridManager ??= new GameObject("GridManager").AddComponent<GridManager>();
                _cameraCalibrationChecker ??= new GameObject("CameraCalibrationChecker").AddComponent<CameraCalibrationChecker>();
                _platformManager ??= new GameObject("PlatformManager").AddComponent<PlatformManager>();
                _planeManager ??= new GameObject("PlaneManager").AddComponent<PlaneManagerWrapper>();
                _exceptionManager ??= new GameObject("ExceptionManager").AddComponent<ExceptionManager>();

                _activityManager = new ActivityManager();
                _augmentationManager = new AugmentationManager();
                _moodleManager = new MoodleManager();
                _editorSceneService = new EditorSceneService();
                _workplaceManager = new WorkplaceManager();
                _openAIManager = new OpenAIManager();
                _aiManager = new AIManager();
                _virtualInstructorManager = new VirtualInstructorManager();

                _exceptionManager.Initialize();
                _brandManager.Initialization();
                await _imageTargetManager.InitializationAsync();
                await _floorManager.InitializationAsync();
                _calibrationManager.Initialization();
                await _pointCloudManager.InitializationAsync();
                await _planeManager.InitializationAsync();
                _gridManager.Initialization();
                _cameraCalibrationChecker.Initialization();
                _platformManager.Initialization();
                await _openAIManager.InitializeAsync();
                await _aiManager.InitializeAsync();
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
            await _planeManager.ResetAsync();
            await _pointCloudManager.ResetAsync();
            await _imageTargetManager.ResetAsync();
        }

        private void OnDestroy()
        {
            if (!_isInitialized)
            {
                return;
            }

            _activityManager.Unsubscribe();
            _pointCloudManager.Unsubscribe();
            _activityManager.OnDestroy();
            _planeManager.Dispose();
            Instance = null;
        }
    }
}
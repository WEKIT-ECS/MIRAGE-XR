using System;
using System.Threading.Tasks;
using MirageXR.NewDataModel;
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

        private ActivityManager _activityManagerOld;
        private AugmentationManager _augmentationManager;
        private MoodleManager _moodleManager;
        private EditorSceneService _editorSceneService;
        private WorkplaceManager _workplaceManager;
        private AIManager _aiManager;
        private OpenAIManager _openAIManager;
        private VirtualInstructorManager _virtualInstructorManager;

        private INetworkDataProvider _networkDataProvider;
        private IActivityManager _activityManager;
        private IContentManager _contentManager;

        public Camera BaseCamera => _baseCamera;

        public ImageTargetManagerWrapper ImageTargetManager => _imageTargetManager;

        public CalibrationManager CalibrationManager => _calibrationManager;

        public FloorManagerWrapper FloorManager => _floorManager;

        public PlaneManagerWrapper PlaneManager => _planeManager;

        public BrandManager BrandManager => _brandManager;

        public GridManager GridManager => _gridManager;

        public ActivityManager ActivityManagerOld => _activityManagerOld;

        public AugmentationManager AugmentationManager => _augmentationManager;

        public MoodleManager MoodleManager => _moodleManager;

        public EditorSceneService EditorSceneService => _editorSceneService;

        public WorkplaceManager WorkplaceManager => _workplaceManager;

        public CameraCalibrationChecker CameraCalibrationChecker => _cameraCalibrationChecker;

        public PlatformManager PlatformManager => _platformManager;

        public ExceptionManager ExceptionManager => _exceptionManager;

        public OpenAIManager OpenAIManager => _openAIManager;
        
        public AIManager AIManager => _aiManager;
        
        public VirtualInstructorManager VirtualInstructorManager => _virtualInstructorManager;


        public INetworkDataProvider NetworkDataProvider => _networkDataProvider;
        public IActivityManager ActivityManager => _activityManager;
        public IContentManager ContentManager => _contentManager;

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

                _activityManagerOld = new ActivityManager();
                _augmentationManager = new AugmentationManager();
                _moodleManager = new MoodleManager();
                _editorSceneService = new EditorSceneService();
                _workplaceManager = new WorkplaceManager();
                _openAIManager = new OpenAIManager();
                _aiManager = new AIManager();
                _virtualInstructorManager = new VirtualInstructorManager();
                _activityManager = new MirageXR.NewDataModel.ActivityManager();

                _activityManager = new NewDataModel.ActivityManager();
                _networkDataProvider = new NetworkDataProvider();
                _contentManager = new ContentManager();

                //_networkDataProvider.InitializeAsync();
                //_contentManager.InitializeAsync();
                _activityManager.InitializeAsync(_contentManager, _networkDataProvider);

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
                //await _aiManager.InitializeAsync();
                _activityManagerOld.Subscription();
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

            _activityManagerOld.Unsubscribe();
            _pointCloudManager.Unsubscribe();
            _activityManagerOld.OnDestroy();
            _planeManager.Dispose();
            Instance = null;
        }
    }
}
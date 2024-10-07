using Fusion;
using System;
using System.Threading.Tasks;
using MirageXR.NewDataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace MirageXR
{
    public class RootObject : MonoBehaviour
    {
        public static RootObject Instance { get; private set; }

        [SerializeField] private Camera _baseCamera;

        [SerializeField] private LearningExperienceEngine.LearningExperienceEngine _lee;
        [SerializeField] private MirageXRServiceBootstrapper _serviceBootstrapper;
        [SerializeField] private ImageTargetManagerWrapper _imageTargetManager;
        [SerializeField] private CalibrationManager _calibrationManager;
        [SerializeField] private FloorManagerWrapper _floorManager;
        [SerializeField] private FloorManagerWithFallback _floorManagerWithRaycastFallback;
        [SerializeField] private PlaneManagerWrapper _planeManager;
        [SerializeField] private PointCloudManager _pointCloudManager;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private CameraCalibrationChecker _cameraCalibrationChecker;
        [SerializeField] private PlatformManager _platformManager;
        [SerializeField] private SharingManager _sharingManager;

        private EditorSceneService _editorSceneService;
        [SerializeField] private WorkplaceController _workplaceController; // added with lib-lee migration
        [SerializeField] private ContentAugmentationController _contentController; // added with lib-lee migration

        private AIManager _aiManager;
        private OpenAIManager _openAIManager;
        private VirtualInstructorOrchestrator _virtualInstructorOrchestrator; 
        private IActivityManager _activityManager;
        private IContentManager _contentManager;
        private IStepManager _stepManager;
        private INetworkDataProvider _networkDataProvider;
        private IAssetsManager _assetsManager;

        public Camera BaseCamera => _baseCamera;

        public LearningExperienceEngine.LearningExperienceEngine LEE => _lee;
        public MirageXRServiceBootstrapper serviceBootstrapper => _serviceBootstrapper;
        public ImageTargetManagerWrapper imageTargetManager => _imageTargetManager;
        public CalibrationManager calibrationManager => _calibrationManager;
        public FloorManagerWrapper floorManager => _floorManager;
        public FloorManagerWithFallback floorManagerWithRaycastFallback => _floorManagerWithRaycastFallback;
        public PlaneManagerWrapper planeManager => _planeManager;
        public GridManager gridManager => _gridManager;
        public EditorSceneService editorSceneService => _editorSceneService;
        public WorkplaceController workplaceController => _workplaceController;
        public ContentAugmentationController contentController => _contentController;
        public CameraCalibrationChecker cameraCalibrationChecker => _cameraCalibrationChecker;
        public PlatformManager platformManager => _platformManager;
        public SharingManager sharingManager => _sharingManager;
        public AIManager aiManager => _aiManager;
        public OpenAIManager openAIManager => _openAIManager;
        public VirtualInstructorOrchestrator virtualInstructorOrchestrator => _virtualInstructorOrchestrator;
        public IActivityManager ActivityManager => _activityManager;
        public IContentManager ContentManager => _contentManager;
        public INetworkDataProvider NetworkDataProvider => _networkDataProvider;
        public IStepManager StepManager => _stepManager;
        public IAssetsManager AssetsManager => _assetsManager;

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
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                
                _baseCamera ??= Camera.main;

                _serviceBootstrapper ??= new GameObject("ServiceBootstrapper").AddComponent<MirageXRServiceBootstrapper>();
                _serviceBootstrapper.transform.parent = transform;
                // await _serviceBootstrapper.RegisterServices(); // not allowed, protected

                _lee ??= new GameObject("LearningExperienceEngine").AddComponent<LearningExperienceEngine.LearningExperienceEngine>();
                await _lee.WaitForInitialization();
                //_lee.transform.parent = transform;

                _imageTargetManager ??= new GameObject("ImageTargetManagerWrapper").AddComponent<ImageTargetManagerWrapper>();
                _calibrationManager ??= new GameObject("CalibrationManager").AddComponent<CalibrationManager>();
                _floorManager ??= new GameObject("FloorManagerWrapper").AddComponent<FloorManagerWrapper>();
				_floorManagerWithRaycastFallback ??= new GameObject("FloorManagerWithRaycastFallback").AddComponent<FloorManagerWithFallback>();
				_pointCloudManager ??= new GameObject("PointCloudManager").AddComponent<PointCloudManager>();
                _gridManager ??= new GameObject("GridManager").AddComponent<GridManager>();
                _cameraCalibrationChecker ??= new GameObject("CameraCalibrationChecker").AddComponent<CameraCalibrationChecker>();
                _platformManager ??= new GameObject("PlatformManager").AddComponent<PlatformManager>();
                _sharingManager ??= new GameObject("Sharing Manager").AddComponent<SharingManager>();
                _planeManager ??= new GameObject("PlaneManager").AddComponent<PlaneManagerWrapper>();

                _editorSceneService = new EditorSceneService();

                _workplaceController ??= new GameObject("WorkplaceController").AddComponent<WorkplaceController>();
                _workplaceController.transform.parent = transform;
                _contentController ??= new GameObject("ContentAugmentationController").AddComponent<ContentAugmentationController>();
                _contentController.transform.parent = transform;

                _aiManager = new AIManager();
                _openAIManager = new OpenAIManager();

                _virtualInstructorOrchestrator = new VirtualInstructorOrchestrator();
                _networkDataProvider = new NetworkDataProvider();
                _contentManager = new ContentManager();
                _stepManager = new StepManager();
                _assetsManager = new AssetsManager();
                _activityManager = new ActivityManager();

                await _imageTargetManager.InitializationAsync();
                await _floorManager.InitializationAsync();
                _calibrationManager.InitializationAsync();
                await _pointCloudManager.InitializationAsync();
                await _planeManager.InitializationAsync();
                _gridManager.Initialization();
                _cameraCalibrationChecker.Initialization();
                _platformManager.Initialization();
                _sharingManager.Initialization();

                await _openAIManager.InitializeAsync();
                await _aiManager.InitializeAsync();
                await _assetsManager.InitializeAsync(_networkDataProvider, _activityManager);
                await _stepManager.InitializeAsync(_contentManager, _activityManager);
                await _contentManager.InitializeAsync(_assetsManager, _stepManager, _activityManager);
                await _activityManager.InitializeAsync(_contentManager, _networkDataProvider, _assetsManager, _stepManager, _lee.authManager);

                _isInitialized = true;

                //LearningExperienceEngine.EventManager.OnClearAll += ResetManagers;
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

            //_activityManager.Unsubscribe();
            _pointCloudManager.Unsubscribe();
            //_activityManager.OnDestroy();
            _planeManager.Dispose();
            Instance = null;
        }
    }
}
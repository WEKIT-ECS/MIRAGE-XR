using System;
using System.Threading.Tasks;
using Hacks;
using i5.Toolkit.Core.VerboseLogging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace MirageXR
{
    public class RootObject : MonoBehaviour
    {
        public static RootObject Instance { get; private set; }

        [SerializeField] private Camera _baseCamera;
        [SerializeField] private GameObject _volumeCamera;

        [SerializeField] private LearningExperienceEngine.LearningExperienceEngine _lee;
        [SerializeField] private MirageXRServiceBootstrapper _serviceBootstrapper;
        [SerializeField] private ImageTargetManagerWrapper _imageTargetManager;
        [SerializeField] private FloorManagerWrapper _floorManager;
        [SerializeField] private FloorManagerWithFallback _floorManagerWithRaycastFallback;
        [SerializeField] private PlaneManagerWrapper _planeManager;
        [SerializeField] private PointCloudManager _pointCloudManager;
        [SerializeField] private VolumeCameraManager _volumeCameraManager;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private CameraCalibrationChecker _cameraCalibrationChecker;
        [SerializeField] private PlatformManager _platformManager;
        [SerializeField] private RoomTwinManager _roomTwinManager;
        [SerializeField] private CollaborationManager _collaborationManager;
        [SerializeField] private WorkplaceController _workplaceController; // added with lib-lee migration
        [SerializeField] private ContentAugmentationController _contentController; // added with lib-lee migration

        private OpenAIManager _openAIManager;
        private EditorSceneService _editorSceneService;
        private VirtualInstructorOrchestrator _virtualInstructorOrchestrator;
        private ICalibrationManager _calibrationManager;
        private IAssetBundleManager _assetBundleManager;
        private IViewManager _viewManager;

        public Camera BaseCamera => _baseCamera;
        public GameObject VolumeCamera => _volumeCamera;

        public LearningExperienceEngine.LearningExperienceEngine LEE => _lee;
        public EditorSceneService EditorSceneService => _editorSceneService;
        public MirageXRServiceBootstrapper ServiceBootstrapper => _serviceBootstrapper;
        public ImageTargetManagerWrapper ImageTargetManager => _imageTargetManager;
        public ICalibrationManager CalibrationManager => _calibrationManager;
        public FloorManagerWrapper FloorManager => _floorManager;
        public FloorManagerWithFallback FloorManagerWithRaycastFallback => _floorManagerWithRaycastFallback;
        public PlaneManagerWrapper PlaneManager => _planeManager;
        public GridManager GridManager => _gridManager;
        public WorkplaceController WorkplaceController => _workplaceController;
        public ContentAugmentationController ContentController => _contentController;
        public CameraCalibrationChecker CameraCalibrationChecker => _cameraCalibrationChecker;
        public PlatformManager PlatformManager => _platformManager;
        public RoomTwinManager RoomTwinManager => _roomTwinManager;
        public CollaborationManager CollaborationManager => _collaborationManager;
        public OpenAIManager OpenAIManager => _openAIManager;
        public VirtualInstructorOrchestrator VirtualInstructorOrchestrator => _virtualInstructorOrchestrator;
        public IAssetBundleManager AssetBundleManager => _assetBundleManager;
        public IViewManager ViewManager => _viewManager;

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
#if POLYSPATIAL_SDK_AVAILABLE && VISION_OS
                InstantiateExtensions.Initialize();
#endif

                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Error = (sender, args) =>
                    {
                        AppLog.LogWarning(args.ErrorContext.Error.Message, sender);
                        args.ErrorContext.Handled = true;
                    }
                };

                _baseCamera ??= Camera.main;

                _serviceBootstrapper ??= new GameObject("ServiceBootstrapper").AddComponent<MirageXRServiceBootstrapper>();
                _serviceBootstrapper.transform.parent = transform;
                // await _serviceBootstrapper.RegisterServices(); // not allowed, protected

                _lee ??= new GameObject("LearningExperienceEngine").AddComponent<LearningExperienceEngine.LearningExperienceEngine>();
                //_lee.transform.parent = transform;

                _imageTargetManager ??= new GameObject("ImageTargetManagerWrapper").AddComponent<ImageTargetManagerWrapper>();
                _floorManager ??= new GameObject("FloorManagerWrapper").AddComponent<FloorManagerWrapper>();
				_floorManagerWithRaycastFallback ??= new GameObject("FloorManagerWithRaycastFallback").AddComponent<FloorManagerWithFallback>();
				_pointCloudManager ??= new GameObject("PointCloudManager").AddComponent<PointCloudManager>();
				_volumeCameraManager ??= new GameObject("VolumeCameraManager").AddComponent<VolumeCameraManager>();
                _gridManager ??= new GameObject("GridManager").AddComponent<GridManager>();
                _cameraCalibrationChecker ??= new GameObject("CameraCalibrationChecker").AddComponent<CameraCalibrationChecker>();
                _platformManager ??= new GameObject("PlatformManager").AddComponent<PlatformManager>();
                _planeManager ??= new GameObject("PlaneManager").AddComponent<PlaneManagerWrapper>();

                _editorSceneService = new EditorSceneService();

                _workplaceController ??= new GameObject("WorkplaceController").AddComponent<WorkplaceController>();
                _workplaceController.transform.parent = transform;
                _contentController ??= new GameObject("ContentAugmentationController").AddComponent<ContentAugmentationController>();
                _contentController.transform.parent = transform;

                _assetBundleManager = new AssetBundleManager();
                _openAIManager = new OpenAIManager();

                _calibrationManager = new CalibrationManager();
                _virtualInstructorOrchestrator = new VirtualInstructorOrchestrator();
                _viewManager = new ViewManager();

                await _lee.WaitForInitialization();
                await _assetBundleManager.InitializeAsync();
                await _imageTargetManager.InitializationAsync();
                await _planeManager.InitializationAsync();
                await _floorManager.InitializationAsync();
                await _calibrationManager.InitializationAsync(_assetBundleManager, _lee.AuthorizationManager);
                await _pointCloudManager.InitializationAsync();
                _volumeCameraManager.Initialization();
                _gridManager.Initialization();
                _cameraCalibrationChecker.Initialization();
                _platformManager.Initialization();
                await _roomTwinManager.InitializationAsync();
                await _openAIManager.InitializeAsync();
                _viewManager.Initialize(_lee.ActivityManager, _assetBundleManager, _collaborationManager);
#if FUSION2
                _collaborationManager.Initialize(_lee.AuthorizationManager, _assetBundleManager);
#endif

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
            _lee.Dispose();
            Instance = null;
        }

        public void AddVolumeCamera(GameObject camera)
        {
            _volumeCamera = camera;
        }
    }
}
using System;
using System.Threading.Tasks;
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
        [SerializeField] private SharingManager _sharingManager;
        [SerializeField] private WorkplaceController _workplaceController; // added with lib-lee migration
        [SerializeField] private ContentAugmentationController _contentController; // added with lib-lee migration

        private AIManager _aiManager;
        private OpenAIManager _openAIManager;
        private EditorSceneService _editorSceneService;
        private VirtualInstructorOrchestrator _virtualInstructorOrchestrator;
        private ICalibrationManager _calibrationManager;
        private IAssetBundleManager _assetBundleManager;

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
        public EditorSceneService editorSceneService => _editorSceneService;
        public WorkplaceController WorkplaceController => _workplaceController;
        public ContentAugmentationController ContentController => _contentController;
        public CameraCalibrationChecker CameraCalibrationChecker => _cameraCalibrationChecker;
        public PlatformManager PlatformManager => _platformManager;
        public RoomTwinManager RoomTwinManager => _roomTwinManager;
        public SharingManager SharingManager => _sharingManager;
        public AIManager AiManager => _aiManager;
        public OpenAIManager OpenAIManager => _openAIManager;
        public VirtualInstructorOrchestrator VirtualInstructorOrchestrator => _virtualInstructorOrchestrator;
        public IAssetBundleManager AssetBundleManager => _assetBundleManager;

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
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Error = (sender, args) =>
                    {
                        AppLog.LogError(args.ErrorContext.Error.Message, sender);
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
                _sharingManager ??= new GameObject("Sharing Manager").AddComponent<SharingManager>();
                _planeManager ??= new GameObject("PlaneManager").AddComponent<PlaneManagerWrapper>();

                _editorSceneService = new EditorSceneService();

                _workplaceController ??= new GameObject("WorkplaceController").AddComponent<WorkplaceController>();
                _workplaceController.transform.parent = transform;
                _contentController ??= new GameObject("ContentAugmentationController").AddComponent<ContentAugmentationController>();
                _contentController.transform.parent = transform;

                _assetBundleManager = new AssetBundleManager();
                _aiManager = new AIManager();
                _openAIManager = new OpenAIManager();

                _calibrationManager = new CalibrationManager();
                _virtualInstructorOrchestrator = new VirtualInstructorOrchestrator();

                await _lee.WaitForInitialization();
                await _assetBundleManager.InitializeAsync();
                await _aiManager.InitializeAsync();
                await _imageTargetManager.InitializationAsync();
                await _planeManager.InitializationAsync();
                await _floorManager.InitializationAsync();
                await _calibrationManager.InitializationAsync(_assetBundleManager);
                await _pointCloudManager.InitializationAsync();
                _volumeCameraManager.Initialization();
                _gridManager.Initialization();
                _cameraCalibrationChecker.Initialization();
                _platformManager.Initialization();
                await _roomTwinManager.InitializationAsync();
                _sharingManager.Initialization();
                await _openAIManager.InitializeAsync();

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

        public void AddVolumeCamera(GameObject camera)
        {
            _volumeCamera = camera;
        }
    }
}
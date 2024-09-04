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

        [SerializeField] private LearningExperienceEngine.LearningExperienceEngine _lee;
        [SerializeField] private MirageXRServiceBootstrapper _serviceBootstrapper;
        [SerializeField] private ImageTargetManagerWrapper _imageTargetManager;
        [SerializeField] private CalibrationManager _calibrationManager;
        [SerializeField] private FloorManagerWrapper _floorManager;
        [SerializeField] private PlaneManagerWrapper _planeManager;
        [SerializeField] private PointCloudManager _pointCloudManager;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private CameraCalibrationChecker _cameraCalibrationChecker;
        [SerializeField] private PlatformManager _platformManager;

        private EditorSceneService _editorSceneService;
        [SerializeField] private WorkplaceController _workplaceController; // added with lib-lee migration
        [SerializeField] private ContentAugmentationController _contentController; // added with lib-lee migration

        private AIManager _aiManager;
        private OpenAIManager _openAIManager;
        private VirtualInstructorManager _virtualInstructorManager; 
        private IActivityManager _activityManager;
        private IContentManager _contentManager;
        private INetworkDataProvider _networkDataProvider;

        public Camera BaseCamera => _baseCamera;

        public LearningExperienceEngine.LearningExperienceEngine LEE => _lee;

        public MirageXRServiceBootstrapper ServiceBootstrapper => _serviceBootstrapper;

        public ImageTargetManagerWrapper ImageTargetManager => _imageTargetManager;

        public CalibrationManager CalibrationManager => _calibrationManager;

        public FloorManagerWrapper FloorManager => _floorManager;

        public PlaneManagerWrapper PlaneManager => _planeManager;

        public GridManager GridManager => _gridManager;

        public EditorSceneService EditorSceneService => _editorSceneService;

        public WorkplaceController WorkplaceController => _workplaceController;

        public ContentAugmentationController ContentController => _contentController;

        public CameraCalibrationChecker CameraCalibrationChecker => _cameraCalibrationChecker;

        public PlatformManager PlatformManager => _platformManager;

        public AIManager AIManager => _aiManager;

        public OpenAIManager OpenAIManager => _openAIManager;

        public VirtualInstructorManager VirtualInstructorManager => _virtualInstructorManager;

        public IActivityManager ActivityManager => _activityManager;

        public IContentManager ContentManager => _contentManager;

        public INetworkDataProvider NetworkDataProvider => _networkDataProvider;

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

                _serviceBootstrapper ??= new GameObject("ServiceBootstrapper").AddComponent<MirageXRServiceBootstrapper>();
                _serviceBootstrapper.transform.parent = transform;
                // await _serviceBootstrapper.RegisterServices(); // not allowed, protected

                _lee ??= new GameObject("LearningExperienceEngine").AddComponent<LearningExperienceEngine.LearningExperienceEngine>();
                await _lee.WaitForInitialization();
                //_lee.transform.parent = transform;

                _imageTargetManager ??= new GameObject("ImageTargetManagerWrapper").AddComponent<ImageTargetManagerWrapper>();
                _calibrationManager ??= new GameObject("CalibrationManager").AddComponent<CalibrationManager>();
                _floorManager ??= new GameObject("FloorManagerWrapper").AddComponent<FloorManagerWrapper>();
                _pointCloudManager ??= new GameObject("PointCloudManager").AddComponent<PointCloudManager>();
                _gridManager ??= new GameObject("GridManager").AddComponent<GridManager>();
                _cameraCalibrationChecker ??= new GameObject("CameraCalibrationChecker").AddComponent<CameraCalibrationChecker>();
                _platformManager ??= new GameObject("PlatformManager").AddComponent<PlatformManager>();
                _planeManager ??= new GameObject("PlaneManager").AddComponent<PlaneManagerWrapper>();

                _editorSceneService = new EditorSceneService();

                _workplaceController ??= new GameObject("WorkplaceController").AddComponent<WorkplaceController>();
                _workplaceController.transform.parent = transform;
                _contentController ??= new GameObject("ContentAugmentationController").AddComponent<ContentAugmentationController>();
                _contentController.transform.parent = transform;

                _aiManager = new AIManager();
                _openAIManager = new OpenAIManager();

                _virtualInstructorManager = new VirtualInstructorManager();
                _activityManager = new ActivityManager();

                await _imageTargetManager.InitializationAsync();
                await _floorManager.InitializationAsync();
                _calibrationManager.InitializationAsync();
                await _pointCloudManager.InitializationAsync();
                await _planeManager.InitializationAsync();
                _gridManager.Initialization();
                _cameraCalibrationChecker.Initialization();
                _platformManager.Initialization();

                await _openAIManager.InitializeAsync();
                await _aiManager.InitializeAsync();
                _activityManager.InitializeAsync(_contentManager, _networkDataProvider);
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
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
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private CameraCalibrationChecker _cameraCalibrationChecker;
        [SerializeField] private PlatformManager _platformManager;
        //[SerializeField] private LearningExperienceEngine.ExceptionManager _exceptionManager;

        private EditorSceneService _editorSceneService;
        private WorkplaceController _workplaceController; // added with lib-lee migration
        private ContentAugmentationController _contentController; // added with lib-lee migration

        private AIManager _aiManager;
        private OpenAIManager _openAIManager;
        private VirtualInstructorManager _virtualInstructorManager; 

        public Camera baseCamera => _baseCamera;

        public ImageTargetManagerWrapper imageTargetManager => _imageTargetManager;

        public CalibrationManager calibrationManager => _calibrationManager;

        public FloorManagerWrapper floorManager => _floorManager;

        public PlaneManagerWrapper planeManager => _planeManager;

        public GridManager gridManager => _gridManager;

        public EditorSceneService editorSceneService => _editorSceneService;

        public WorkplaceController workplaceController => _workplaceController;
        public ContentAugmentationController contentController => _contentController;

        public CameraCalibrationChecker cameraCalibrationChecker => _cameraCalibrationChecker;

        public PlatformManager platformManager => _platformManager;

        // public LearningExperienceEngine.ExceptionManager exceptionManager => _exceptionManager;

        public AIManager aiManager => _aiManager;
        public OpenAIManager openAIManager => _openAIManager;

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

                _imageTargetManager ??= new GameObject("ImageTargetManagerWrapper").AddComponent<ImageTargetManagerWrapper>();
                _calibrationManager ??= new GameObject("CalibrationManager").AddComponent<CalibrationManager>();
                _floorManager ??= new GameObject("FloorManagerWrapper").AddComponent<FloorManagerWrapper>();
                _pointCloudManager ??= new GameObject("PointCloudManager").AddComponent<PointCloudManager>();
                _gridManager ??= new GameObject("GridManager").AddComponent<GridManager>();
                _cameraCalibrationChecker ??= new GameObject("CameraCalibrationChecker").AddComponent<CameraCalibrationChecker>();
                _platformManager ??= new GameObject("PlatformManager").AddComponent<PlatformManager>();
                _planeManager ??= new GameObject("PlaneManager").AddComponent<PlaneManagerWrapper>();
                // _exceptionManager ??= new GameObject("ExceptionManager").AddComponent<LearningExperienceEngine.ExceptionManager>();

                _editorSceneService = new EditorSceneService();
                _workplaceController = new WorkplaceController();
                _contentController = new ContentAugmentationController();

                _aiManager = new AIManager();
                _openAIManager = new OpenAIManager();

                _virtualInstructorManager = new VirtualInstructorManager();

                //_exceptionManager.Initialize();
                
                await _imageTargetManager.InitializationAsync();
                await _floorManager.InitializationAsync();
                await _calibrationManager.InitializationAsync();
                await _pointCloudManager.InitializationAsync();
                await _planeManager.InitializationAsync();
                _gridManager.Initialization();
                _cameraCalibrationChecker.Initialization();
                _platformManager.Initialization();

                await _openAIManager.InitializeAsync();
                await _aiManager.InitializeAsync();
                
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

            //_activityManager.Unsubscribe();
            _pointCloudManager.Unsubscribe();
            //_activityManager.OnDestroy();
            _planeManager.Dispose();
            Instance = null;
        }
    }
}
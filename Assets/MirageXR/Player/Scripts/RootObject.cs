using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.XR.Management;

namespace MirageXR
{
	public class RootObject : MonoBehaviour
	{
		public static RootObject Instance { get; private set; }

		[SerializeField] private LearningExperienceEngine.LearningExperienceEngine _lee;
		[SerializeField] private ImageTargetManagerWrapper _imageTargetManager;
		[SerializeField] private FloorManagerWrapper _floorManager;
		[SerializeField] private FloorManagerWithFallback _floorManagerWithRaycastFallback;
		[SerializeField] private PlaneManagerWrapper _planeManager;
		[SerializeField] private PointCloudManager _pointCloudManager;
		[SerializeField] private GridManager _gridManager;
		[SerializeField] private CameraCalibrationChecker _cameraCalibrationChecker;
		[SerializeField] private PlatformManager _platformManager;
		[SerializeField] private RoomTwinManager _roomTwinManager;
		[SerializeField] private CollaborationManager _collaborationManager;
		[SerializeField] private WorkplaceController _workplaceController; // added with lib-lee migration
		[SerializeField] private ContentAugmentationController _contentController; // added with lib-lee migration
		[SerializeField] private AvatarLibraryManager _avatarLibraryManager;

		private OpenAIManager _openAIManager;
		private EditorSceneService _editorSceneService;
		private VirtualInstructorOrchestrator _virtualInstructorOrchestrator;
		private ICalibrationManager _calibrationManager;
		private IAssetBundleManager _assetBundleManager;
        private IViewManager _viewManager;
		private MirageXRServiceBootstrapper _serviceBootstrapper;

		public Camera BaseCamera => _viewManager.Camera;

		public LearningExperienceEngine.LearningExperienceEngine LEE => _lee;
		public EditorSceneService EditorSceneService => _editorSceneService;
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
		public AvatarLibraryManager AvatarLibraryManager => _avatarLibraryManager;
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
		{                                   //TODO: initialize all managers asynchronously.
			UnityEngine.Debug.Log("Initializing [RootObject] <--");
			if (_isInitialized)
			{
				return;
			}

			try
			{
// #if POLYSPATIAL_SDK_AVAILABLE && VISION_OS
//                 InstantiateExtensions.Initialize();
// #endif

				JsonConvert.DefaultSettings = () => new JsonSerializerSettings
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					Error = (sender, args) =>
					{
						AppLog.LogWarning(args.ErrorContext.Error.Message, sender);
						args.ErrorContext.Handled = true;
					}
				};

				_serviceBootstrapper ??= new GameObject("ServiceBootstrapper").AddComponent<MirageXRServiceBootstrapper>();
				_serviceBootstrapper.transform.parent = transform;
				// await _serviceBootstrapper.RegisterServices(); // not allowed, protected

				_lee ??= new GameObject("LearningExperienceEngine").AddComponent<LearningExperienceEngine.LearningExperienceEngine>();
				//_lee.transform.parent = transform;

				_imageTargetManager ??= new GameObject("ImageTargetManagerWrapper").AddComponent<ImageTargetManagerWrapper>();
				_floorManager ??= new GameObject("FloorManagerWrapper").AddComponent<FloorManagerWrapper>();
				_floorManagerWithRaycastFallback ??= new GameObject("FloorManagerWithRaycastFallback").AddComponent<FloorManagerWithFallback>();
				_pointCloudManager ??= new GameObject("PointCloudManager").AddComponent<PointCloudManager>();
				_gridManager ??= new GameObject("GridManager").AddComponent<GridManager>();
				_cameraCalibrationChecker ??= new GameObject("CameraCalibrationChecker").AddComponent<CameraCalibrationChecker>();
				_platformManager ??= new GameObject("PlatformManager").AddComponent<PlatformManager>();
				_planeManager ??= new GameObject("PlaneManager").AddComponent<PlaneManagerWrapper>();
				_avatarLibraryManager ??= new GameObject("AvatarLibraryManager").AddComponent<AvatarLibraryManager>();

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

				await _assetBundleManager.InitializeAsync();
				_viewManager.Initialize(_lee.ActivityManager, _assetBundleManager, _platformManager, _collaborationManager);
				_lee.InitializeAsync().Forget();
				await _lee.WaitForInitialization();
				await _imageTargetManager.InitializationAsync(_viewManager);
				await _planeManager.InitializationAsync(_viewManager);
				await _floorManager.InitializationAsync(_viewManager, _planeManager);
				await _calibrationManager.InitializationAsync(_assetBundleManager, _lee.AuthorizationManager);
				await _pointCloudManager.InitializationAsync(_viewManager);
				_gridManager.Initialization();
				_cameraCalibrationChecker.Initialization(_viewManager);
				_platformManager.Initialization();
				await _roomTwinManager.InitializationAsync();
				await _openAIManager.InitializeAsync();
#if FUSION2
                _collaborationManager.Initialize(_lee.AuthorizationManager, _assetBundleManager);
#endif
				await StartXR();

				_isInitialized = true;
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
			UnityEngine.Debug.Log("Initializing [RootObject] -->");
		}

		private async UniTask StartXR()
		{
			UnityEngine.Debug.Log("Initializing XR");
			await XRGeneralSettings.Instance.Manager.InitializeLoader();
			XRGeneralSettings.Instance.Manager.StartSubsystems();
			UnityEngine.Debug.Log("XR started");
		}

		public void StopXR()
		{
			Debug.Log("Stopping XR...");
			XRGeneralSettings.Instance.Manager.StopSubsystems();
			XRGeneralSettings.Instance.Manager.DeinitializeLoader();
		}
		
		private void ResetManagers()
		{
			ResetManagersAsync().Forget();
		}

		private async UniTask ResetManagersAsync()
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
	}
}
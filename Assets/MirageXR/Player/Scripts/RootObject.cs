using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class RootObject : MonoBehaviour
    {
        [SerializeField] private ImageTargetManagerWrapper _imageTargetManager;
        [SerializeField] private CalibrationManager _calibrationManager;
        private ActivityManager _activityManager;
        private AugmentationManager _augmentationManager;
        private MoodleManager _moodleManager;
        private EditorSceneService _editorSceneService;
        private WorkplaceManager _workplaceManager;

        public ImageTargetManagerWrapper imageTargetManager => _imageTargetManager;

        public CalibrationManager calibrationManager => _calibrationManager;

        public ActivityManager activityManager => _activityManager;

        public AugmentationManager augmentationManager => _augmentationManager;

        public MoodleManager moodleManager => _moodleManager;

        public EditorSceneService editorSceneService => _editorSceneService;

        public WorkplaceManager workplaceManager => _workplaceManager;


        public static RootObject Instance { get; private set; }

        private bool _isInitialized;

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

            _activityManager = new ActivityManager();
            _augmentationManager = new AugmentationManager();
            _moodleManager = new MoodleManager();
            _editorSceneService = new EditorSceneService();
            _workplaceManager = new WorkplaceManager();

            await _imageTargetManager.InitializationAsync();
            calibrationManager.Initialization();

            _activityManager.Subscription();

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            _activityManager.Unsubscribe();
            _activityManager.OnDestroy();
        }
    }
}
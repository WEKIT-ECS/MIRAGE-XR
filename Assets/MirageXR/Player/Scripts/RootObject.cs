using UnityEngine;

namespace MirageXR
{
    public class RootObject : MonoBehaviour
    {
        public ActivityManager activityManager;
        public AugmentationManager augmentationManager;
        public MoodleManager moodleManager;
        public EditorSceneService editorSceneService;
        public WorkplaceManager workplaceManager;

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
            Initialization();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Initialization()
        {
            if (_isInitialized) return;

            activityManager = new ActivityManager();
            augmentationManager = new AugmentationManager();
            moodleManager = new MoodleManager();
            editorSceneService = new EditorSceneService();
            workplaceManager = new WorkplaceManager();

            activityManager.Subscription();
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            activityManager.Unsubscribe();
            activityManager.OnDestroy();
        }
    }
}
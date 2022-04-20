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

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Init();
            DontDestroyOnLoad(gameObject);
        }

        private void Init()
        {
            activityManager = new ActivityManager();
            augmentationManager = new AugmentationManager();
            moodleManager = new MoodleManager();
            editorSceneService = new EditorSceneService();
            workplaceManager = new WorkplaceManager();
            
            activityManager.Subscription();
        }

        private void OnDestroy()
        {
            activityManager.Unsubscribe();
            activityManager.OnDestroy();
        }
    }
}
using System.IO;
using ReadyPlayerMe.Core.Analytics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class IntegrationGuide : EditorWindow
    {
        private const string INTEGRATION_GUIDE = "Integration Guide";
        private const string HEADER_LABEL = "HeaderLabel";
        private const string LOAD_AVATARS_URL = "https://docs.readyplayer.me/ready-player-me/integration-guides/unity/load-avatars";
        private const string ADD_ANIMATION_URL = "https://docs.readyplayer.me/ready-player-me/integration-guides/unity/animations";
        private const string AVATAR_CREATOR_URL = "https://docs.readyplayer.me/ready-player-me/integration-guides/unity/avatar-creator";
        private const string OPTIMIZE_PERFORMANCE_URL = "https://docs.readyplayer.me/ready-player-me/integration-guides/unity/optimize";
        private const string QUICK_START = "QuickStart";
        private const string LOAD_AVATARS = "LoadAvatars";
        private const string ADD_ANIMATIONS = "AddAnimations";
        private const string INTEGRATE_AVATAR_CREATOR = "IntegrateAvatarCreator";
        private const string OPTIMIZE_THE_PERFORMANCE = "OptimizeThePerformance";
        private const string CORE_PACKAGE = "com.readyplayerme.core";
        private const string QUICKSTART_SAMPLE_NAME = "QuickStart";
        private const string AVATAR_CREATOR_SAMPLE_NAME = "AvatarCreatorSamples";
        private const string AVATAR_CREATOR_SAMPLE_SCENE_PATH = "AvatarCreatorElements/Scenes/AvatarCreatorElements";
        private const string SAMPLES_FOLDER_PATH = "Assets/Ready Player Me/Core/Samples";
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        [MenuItem("Tools/Ready Player Me/Integration Guide", priority = 12)]
        public static void ShowWindow()
        {
            var window = GetWindow<IntegrationGuide>();
            window.titleContent = new GUIContent(INTEGRATION_GUIDE);
            window.minSize = new Vector2(500, 530);
            AnalyticsEditorLogger.EventLogger.LogOpenIntegrationGuide();
        }

        public void CreateGUI()
        {
            visualTreeAsset.CloneTree(rootVisualElement);

            var headerLabel = rootVisualElement.Q<Label>(HEADER_LABEL);
            headerLabel.text = INTEGRATION_GUIDE;

            RegisterButtons();
        }

        private void RegisterButtons()
        {
            rootVisualElement.Q<VisualElement>(QUICK_START).Q<Button>().clicked += () =>
            {
                AnalyticsEditorLogger.EventLogger.LogLoadQuickStartScene();
                LoadAndOpenSample(QUICKSTART_SAMPLE_NAME, QUICKSTART_SAMPLE_NAME);
            };

            rootVisualElement.Q<VisualElement>(LOAD_AVATARS).Q<Button>().clicked += () =>
            {
                AnalyticsEditorLogger.EventLogger.LogOpenAvatarDocumentation();
                OpenDocumentation(LOAD_AVATARS_URL);
            };

            rootVisualElement.Q<VisualElement>(ADD_ANIMATIONS).Q<Button>().clicked += () =>
            {
                AnalyticsEditorLogger.EventLogger.LogOpenAnimationDocumentation();
                OpenDocumentation(ADD_ANIMATION_URL);
            };

            rootVisualElement.Q<VisualElement>(INTEGRATE_AVATAR_CREATOR).Q<Button>("SeeDocsButton").clicked += () =>
            {
                AnalyticsEditorLogger.EventLogger.LogOpenAvatarCreatorDocumentation();
                OpenDocumentation(AVATAR_CREATOR_URL);
            };

            rootVisualElement.Q<VisualElement>(INTEGRATE_AVATAR_CREATOR).Q<Button>("LoadSampleSceneButton").clicked += () =>
            {
                AnalyticsEditorLogger.EventLogger.LogAvatarCreatorSampleImported();
                LoadAndOpenSample(AVATAR_CREATOR_SAMPLE_NAME, AVATAR_CREATOR_SAMPLE_SCENE_PATH);
            };

            rootVisualElement.Q<VisualElement>(OPTIMIZE_THE_PERFORMANCE).Q<Button>().clicked += () =>
            {
                AnalyticsEditorLogger.EventLogger.LogOpenOptimizationDocumentation();
                OpenDocumentation(OPTIMIZE_PERFORMANCE_URL);
            };
        }

        private void LoadAndOpenSample(string sampleName, string scenePath)
        {
            if (LoadFromAssetsFolder(sampleName, scenePath))
            {
                return;
            }

            var sampleLoader = new SampleLoader();

            if (sampleLoader.Load(CORE_PACKAGE, sampleName))
            {
                sampleLoader.OpenScene(scenePath);
                return;
            }

            EditorUtility.DisplayDialog(INTEGRATION_GUIDE, $"No sample with name {sampleName} found.", "OK");
        }

        private bool LoadFromAssetsFolder(string sampleName, string scenePath)
        {
            var version = ApplicationData.GetData().UnityVersion;

            var folderPath = $"{SAMPLES_FOLDER_PATH}/{sampleName}";
            if (!Directory.Exists(folderPath))
            {
                folderPath = $"{SAMPLES_FOLDER_PATH}/{version}/{sampleName}";
            }
            if (!Directory.Exists(folderPath)) return false;
            var fullScenePath = $"{folderPath}/{scenePath}.unity";
            if (!File.Exists(fullScenePath)) return false;
            EditorSceneManager.OpenScene(fullScenePath);
            return true;
        }

        private void OpenDocumentation(string link)
        {
            Application.OpenURL(link);
        }
    }
}

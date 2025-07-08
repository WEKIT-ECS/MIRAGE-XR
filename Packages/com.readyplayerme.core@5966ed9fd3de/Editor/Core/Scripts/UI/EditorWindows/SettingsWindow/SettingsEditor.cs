using System.IO;
using ReadyPlayerMe.Core.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class SettingsEditor : EditorWindow
    {
        private const string SETTINGS = "Settings";
        private const string HEADER_LABEL = "HeaderLabel";
        private const string CACHING_TOOLTIP =
            "Enable caching to improve avatar loading performance at runtime.";
        private const string AVATAR_CACHING_HEADING = "AvatarCachingHeading";
        private const string AVATAR_CACHING_HELP_BUTTON = "AvatarCachingHelpButton";
        private const string AVATAR_CACHING_ENABLED_TOGGLE = "AvatarCachingEnabledToggle";
        private const string CLEAR_CACHE_BUTTON = "ClearCacheButton";
        private const string SHOW_CACHE_BUTTON = "ShowCacheButton";
        private const string ANALYTICS_ENABLED_TOGGLE = "AnalyticsEnabledToggle";
        private const string LOGGING_ENABLED_TOGGLE = "LoggingEnabledToggle";
        private const string DOCUMENTATION_BUTTON = "DocumentationButton";
        private const string FAQ_BUTTON = "FaqButton";
        private const string FORUM_BUTTON = "ForumButton";
        private const string CLEAR_CACHE = "Clear Cache";
        private const string CACHE_IS_ALREADY_EMPTY = "Cache is already empty";
        private const string OK = "OK";
        private const string CANCEL = "Cancel";

#if UNITY_EDITOR_LINUX
        private const string SHOW_CACHING_FOLDER_BUTTON_TEXT = "Show in file manager";
#elif UNITY_EDITOR_OSX
        private const string SHOW_CACHING_FOLDER_BUTTON_TEXT = "Reveal in finder";
#else
        private const string SHOW_CACHING_FOLDER_BUTTON_TEXT = "Show in explorer";
#endif

        private const string ANALYTICS_PRIVACY_URL = "https://docs.readyplayer.me/ready-player-me/integration-guides/unity/help-us-improve-the-unity-sdk";

        private const string DOCS_URL = "https://docs.readyplayer.me/ready-player-me/integration-guides/unity";
        private const string FAQ_URL = "https://docs.readyplayer.me/ready-player-me/integration-guides/unity/faq-for-unity";
        private const string FORUM_URL = "https://forum.readyplayer.me/";
        private const string PRIVACY_POLICY_LABEL = "PrivacyPolicyLabel";

        [SerializeField] private VisualTreeAsset visualTreeAsset;

        private bool isCacheEmpty;
        private Button clearCacheButton;

        [MenuItem("Tools/Ready Player Me/Settings", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<SettingsEditor>();
            window.titleContent = new GUIContent(SETTINGS);
            window.minSize = new Vector2(500, 620);

            AnalyticsEditorLogger.EventLogger.LogOpenDialog(SETTINGS);
        }

        public void CreateGUI()
        {
            visualTreeAsset.CloneTree(rootVisualElement);

            var headerLabel = rootVisualElement.Q<Label>(HEADER_LABEL);
            headerLabel.text = SETTINGS;

            isCacheEmpty = AvatarCache.IsCacheEmpty();

            rootVisualElement.Q(AVATAR_CACHING_HEADING).tooltip = CACHING_TOOLTIP;

            rootVisualElement.Q<Button>(AVATAR_CACHING_HELP_BUTTON).clicked += OnCachingHelpClick;

            var avatarCachingToggle = rootVisualElement.Q<Toggle>(AVATAR_CACHING_ENABLED_TOGGLE);
            avatarCachingToggle.value = AvatarLoaderSettingsHelper.AvatarLoaderSettings.AvatarCachingEnabled;
            avatarCachingToggle.RegisterValueChangedCallback(OnAvatarCachingToggle);

            clearCacheButton = rootVisualElement.Q<Button>(CLEAR_CACHE_BUTTON);
            clearCacheButton.clicked += TryClearCache;
            clearCacheButton.SetEnabled(!isCacheEmpty);

            var showCacheButton = rootVisualElement.Q<Button>(SHOW_CACHE_BUTTON);
            showCacheButton.text = SHOW_CACHING_FOLDER_BUTTON_TEXT;
            showCacheButton.clicked += ShowCacheDirectory;

            var analyticsEnabledToggle = rootVisualElement.Q<Toggle>(ANALYTICS_ENABLED_TOGGLE);
            analyticsEnabledToggle.value = AnalyticsEditorLogger.IsEnabled;
            analyticsEnabledToggle.RegisterValueChangedCallback(OnAnalyticsToggled);

            rootVisualElement.Q<Label>(PRIVACY_POLICY_LABEL).RegisterCallback<MouseUpEvent>(OnPrivacyPolicyClicked);

            var loggingEnabledToggle = rootVisualElement.Q<Toggle>(LOGGING_ENABLED_TOGGLE);
            loggingEnabledToggle.value = SDKLogger.IsLoggingEnabled();
            loggingEnabledToggle.RegisterValueChangedCallback(OnLoggingToggle);

            rootVisualElement.Q<Button>(DOCUMENTATION_BUTTON).clicked += () => Application.OpenURL(DOCS_URL);
            rootVisualElement.Q<Button>(FAQ_BUTTON).clicked += () => Application.OpenURL(FAQ_URL);
            rootVisualElement.Q<Button>(FORUM_BUTTON).clicked += () => Application.OpenURL(FORUM_URL);
        }

        private void OnCachingHelpClick()
        {
            AnalyticsEditorLogger.EventLogger.LogFindOutMore(HelpSubject.AvatarCaching);
            Application.OpenURL(Constants.Links.DOCS_AVATAR_CACHING);
        }

        private void OnAvatarCachingToggle(ChangeEvent<bool> evt)
        {
            AvatarLoaderSettingsHelper.SetAvatarCaching(evt.newValue);
            AnalyticsEditorLogger.EventLogger.LogSetCachingEnabled(evt.newValue);
        }

        private void OnLoggingToggle(ChangeEvent<bool> evt)
        {
            AnalyticsEditorLogger.EventLogger.LogSetLoggingEnabled(evt.newValue);
            SDKLogger.EnableLogging(evt.newValue);
            CoreSettingsSetter.SetEnableLogging(evt.newValue);
        }

        private void OnPrivacyPolicyClicked(MouseUpEvent evt)
        {
            AnalyticsEditorLogger.EventLogger.LogViewPrivacyPolicy();
            Application.OpenURL(ANALYTICS_PRIVACY_URL);
        }

        private void OnFocus()
        {
            isCacheEmpty = AvatarCache.IsCacheEmpty();
            clearCacheButton?.SetEnabled(!isCacheEmpty);
        }

        private void OnAnalyticsToggled(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                AnalyticsEditorLogger.Enable();
            }
            else
            {
                AnalyticsEditorLogger.Disable();
            }
        }

        private void TryClearCache()
        {
            AnalyticsEditorLogger.EventLogger.LogClearLocalCache();
            if (isCacheEmpty)
            {
                EditorUtility.DisplayDialog(CLEAR_CACHE, CACHE_IS_ALREADY_EMPTY, OK);
                return;
            }
            var size = (AvatarCache.GetCacheSize() / (1024f * 1024)).ToString("F2");
            var avatarCount = AvatarCache.GetAvatarCount();
            if (EditorUtility.DisplayDialog(CLEAR_CACHE, $"Do you want to clear all the Avatars cache from persistent data path, {size} MB and {avatarCount} avatars?", OK, CANCEL))
            {
                AvatarCache.Clear();
            }
            isCacheEmpty = true;
        }

        private void ShowCacheDirectory()
        {
            AnalyticsEditorLogger.EventLogger.LogShowInExplorer();
            var path = DirectoryUtility.GetAvatarsPersistantPath();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            EditorUtility.RevealInFinder(path);
        }
    }
}

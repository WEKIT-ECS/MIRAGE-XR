using ReadyPlayerMe.Core.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    public class AvatarLoaderEditor : EditorWindow
    {
        private const string TAG = nameof(AvatarLoaderEditor);
        private const string AVATAR_LOADER = "Avatar Loader";
        private const string LOAD_AVATAR_BUTTON = "LoadAvatarButton";
        private const string HEADER_LABEL = "HeaderLabel";
        private const string USE_EYE_ANIMATIONS_TOGGLE = "UseEyeAnimationsToggle";
        private const string USE_VOICE_TO_ANIMATION_TOGGLE = "UseVoiceToAnimationToggle";

        private const string VOICE_TO_ANIM_SAVE_KEY = "VoiceToAnimSaveKey";
        private const string EYE_ANIMATION_SAVE_KEY = "EyeAnimationSaveKey";
        private const string MODEL_CACHING_SAVE_KEY = "ModelCachingSaveKey";

        [SerializeField] private VisualTreeAsset visualTreeAsset;

        private double startTime;
        private AvatarLoaderSettings avatarLoaderSettings;

        private bool useEyeAnimations;
        private bool useVoiceToAnim;

        [MenuItem("Tools/Ready Player Me/Avatar Loader", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<AvatarLoaderEditor>();
            window.titleContent = new GUIContent(AVATAR_LOADER);
            window.minSize = new Vector2(500, 300);
        }

        public void CreateGUI()
        {
            visualTreeAsset.CloneTree(rootVisualElement);

            if (EditorPrefs.GetBool(MODEL_CACHING_SAVE_KEY)) EditorPrefs.SetBool(MODEL_CACHING_SAVE_KEY, false);

            var headerLabel = rootVisualElement.Q<Label>(HEADER_LABEL);
            headerLabel.text = AVATAR_LOADER;

            var useEyeAnimationsToggle = rootVisualElement.Q<Toggle>(USE_EYE_ANIMATIONS_TOGGLE);
            useEyeAnimations = EditorPrefs.GetBool(EYE_ANIMATION_SAVE_KEY);
            useEyeAnimationsToggle.value = useEyeAnimations;
            useEyeAnimationsToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                useEyeAnimations = evt.newValue;
                EditorPrefs.SetBool(EYE_ANIMATION_SAVE_KEY, useEyeAnimations);
            });

            var useVoiceToAnimToggle = rootVisualElement.Q<Toggle>(USE_VOICE_TO_ANIMATION_TOGGLE);
            useVoiceToAnim = EditorPrefs.GetBool(VOICE_TO_ANIM_SAVE_KEY);
            useVoiceToAnimToggle.value = useVoiceToAnim;
            useVoiceToAnimToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                useVoiceToAnim = evt.newValue;
                EditorPrefs.SetBool(VOICE_TO_ANIM_SAVE_KEY, useVoiceToAnim);
            });

            var avatarLoader = rootVisualElement.Q<Button>(LOAD_AVATAR_BUTTON);
            var urlField = rootVisualElement.Q<AvatarUrlTemplate>();

            avatarLoader.clicked += () =>
            {
                if (urlField.TryGetUrl(out var url))
                {
                    LoadAvatar(url);
                }
            };
        }

        private void LoadAvatar(string url)
        {
            startTime = EditorApplication.timeSinceStartup;

            AnalyticsEditorLogger.EventLogger.LogLoadAvatarFromDialog(url, useEyeAnimations, useVoiceToAnim);
            if (avatarLoaderSettings == null)
            {
                avatarLoaderSettings = AvatarLoaderSettings.LoadSettings();
            }
            var avatarLoader = new AvatarObjectLoader();
            avatarLoader.OnFailed += Failed;
            avatarLoader.OnCompleted += Completed;
            avatarLoader.OperationCompleted += OnOperationCompleted;
            if (avatarLoaderSettings != null)
            {
                avatarLoader.AvatarConfig = avatarLoaderSettings.AvatarConfig;
                if (avatarLoaderSettings.GLTFDeferAgent != null)
                {
                    avatarLoader.GLTFDeferAgent = avatarLoaderSettings.GLTFDeferAgent;
                }
            }
            avatarLoader.LoadAvatar(url);
        }

        private void OnOperationCompleted(object sender, IOperation<AvatarContext> e)
        {
            if (e.GetType() == typeof(MetadataDownloader))
            {
                AnalyticsEditorLogger.EventLogger.LogMetadataDownloaded(EditorApplication.timeSinceStartup - startTime);
            }
        }

        private void Failed(object sender, FailureEventArgs args)
        {
            Debug.LogError($"{args.Type} - {args.Message}");
        }

        private void Completed(object sender, CompletionEventArgs args)
        {
            AnalyticsEditorLogger.EventLogger.LogAvatarLoaded(EditorApplication.timeSinceStartup - startTime);

            if (avatarLoaderSettings == null)
            {
                avatarLoaderSettings = AvatarLoaderSettings.LoadSettings();
            }
            var paramHash = AvatarCache.GetAvatarConfigurationHash(avatarLoaderSettings.AvatarConfig);
            var path = $"{DirectoryUtility.GetRelativeProjectPath(args.Avatar.name, paramHash)}/{args.Avatar.name}";
            if (!avatarLoaderSettings.AvatarCachingEnabled)
            {
                SDKLogger.LogWarning(TAG, "Enable Avatar Caching to generate a prefab in the project folder.");
                return;
            }
            var avatar = PrefabHelper.CreateAvatarPrefab(args.Metadata, path, avatarConfig: avatarLoaderSettings.AvatarConfig);
            if (useEyeAnimations) avatar.AddComponent<EyeAnimationHandler>();
            if (useVoiceToAnim) avatar.AddComponent<VoiceHandler>();
            DestroyImmediate(args.Avatar, true);
            Selection.activeObject = avatar;
        }
    }
}

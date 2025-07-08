using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.Core.Editor
{
    [InitializeOnLoad]
    public class AvatarLoaderSettingsCreator
    {
        private const string SETTINGS_SAVE_FOLDER = "Ready Player Me/Resources/Settings";
        private const string AVATAR_LOADER_ASSET_NAME = "AvatarLoaderSettings.asset";

        static AvatarLoaderSettingsCreator()
        {
            EditorApplication.delayCall += CreateSettingsAssets;
        }

        ~AvatarLoaderSettingsCreator()
        {
            EditorApplication.delayCall -= CreateSettingsAssets;
        }

        private static void CreateSettingsAssets()
        {
            if (AvatarLoaderSettings.LoadSettings() != null)
            {
                return;
            }
            DirectoryUtility.ValidateDirectory($"{Application.dataPath}/{SETTINGS_SAVE_FOLDER}");
            AssetDatabase.Refresh();
            CreateAvatarLoaderSettings();
        }

        private static void CreateAvatarLoaderSettings()
        {
            var newSettings = ScriptableObject.CreateInstance<AvatarLoaderSettings>();
            newSettings.AvatarConfig = null;
            newSettings.GLTFDeferAgent = null;
            newSettings.AvatarCachingEnabled = true;

            AssetDatabase.CreateAsset(newSettings, $"Assets/{SETTINGS_SAVE_FOLDER}/{AVATAR_LOADER_ASSET_NAME}");
            AssetDatabase.SaveAssets();
        }
    }
}

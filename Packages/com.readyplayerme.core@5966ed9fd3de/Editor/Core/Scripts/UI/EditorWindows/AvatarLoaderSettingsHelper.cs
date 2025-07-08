using UnityEditor;

namespace ReadyPlayerMe.Core.Editor
{
    public static class AvatarLoaderSettingsHelper
    {
        public static AvatarLoaderSettings AvatarLoaderSettings
        {
            get
            {
                if (avatarLoaderSettings == null)
                {
                    avatarLoaderSettings = AvatarLoaderSettings.LoadSettings();
                }

                return avatarLoaderSettings;
            }
        }

        private static AvatarLoaderSettings avatarLoaderSettings;

        public static void SaveAvatarConfig(AvatarConfig avatarConfig)
        {
            if (avatarLoaderSettings != null && avatarLoaderSettings.AvatarConfig != avatarConfig)
            {
                avatarLoaderSettings.AvatarConfig = avatarConfig;
                SaveAvatarLoaderSettings();
            }
        }
        
        public static void SaveDeferAgent(GLTFDeferAgent deferAgent)
        {
            if (avatarLoaderSettings != null && avatarLoaderSettings.GLTFDeferAgent != deferAgent)
            {
                avatarLoaderSettings.GLTFDeferAgent = deferAgent;
                SaveAvatarLoaderSettings();
            }
        }
        
        public static void SetAvatarCaching(bool enable)
        {
            if (avatarLoaderSettings != null)
            {
                avatarLoaderSettings.AvatarCachingEnabled = enable;
                SaveAvatarLoaderSettings();
            }
        }
        
        private static void SaveAvatarLoaderSettings()
        {
            EditorUtility.SetDirty(avatarLoaderSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

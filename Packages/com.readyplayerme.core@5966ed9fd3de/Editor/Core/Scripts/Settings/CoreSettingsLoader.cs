using System.IO;
using ReadyPlayerMe.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.Core.Editor
{
    [InitializeOnLoad]
    public static class CoreSettingsLoader
    {
        private const string PROJECT_RELATIVE_ASSET_PATH = "Assets/Ready Player Me/Resources/Settings/CoreSettings.asset";
        private const string SETTINGS_SAVE_FOLDER = "Ready Player Me/Resources/Settings";

        static CoreSettingsLoader()
        {
            EnsureSettingsExist();
        }

        public static void EnsureSettingsExist()
        {
            if (CoreSettingsHandler.CoreSettings == null && !File.Exists($"{Application.dataPath}/Ready Player Me/Resources/Settings/CoreSettings.asset"))
            {
                CreateSettings();
            }
        }

        private static void CreateSettings()
        {
            DirectoryUtility.ValidateDirectory($"{Application.dataPath}/{SETTINGS_SAVE_FOLDER}");
            var newSettings = ScriptableObject.CreateInstance<CoreSettings>();
            AssetDatabase.CreateAsset(newSettings, PROJECT_RELATIVE_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

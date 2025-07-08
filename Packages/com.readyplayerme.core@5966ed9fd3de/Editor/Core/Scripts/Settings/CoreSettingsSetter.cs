using UnityEditor;

namespace ReadyPlayerMe.Core.Editor
{
    public static class CoreSettingsSetter
    {
        public static void SetEnableAnalytics(bool isEnabled)
        {
            CoreSettingsHandler.CoreSettings.EnableAnalytics = isEnabled;
            Save();
        }

        public static void SetEnableLogging(bool isEnabled)
        {
            CoreSettingsHandler.CoreSettings.EnableLogging = isEnabled;
            Save();
        }

        public static void SaveSubDomain(string subDomain)
        {
            if (string.IsNullOrEmpty(subDomain) || CoreSettingsHandler.CoreSettings.Subdomain == subDomain) return;
            CoreSettingsHandler.CoreSettings.Subdomain = subDomain;
            Save();
        }

        public static void SaveAppId(string appId)
        {
            CoreSettingsHandler.CoreSettings.AppId = appId;
            Save();
        }

        public static void SaveBodyType(BodyType bodyType)
        {
            CoreSettingsHandler.CoreSettings.BodyType = bodyType;
            Save();
        }

        public static void Save()
        {
            EditorUtility.SetDirty(CoreSettingsHandler.CoreSettings);
            AssetDatabase.SaveAssets();
        }
    }
}

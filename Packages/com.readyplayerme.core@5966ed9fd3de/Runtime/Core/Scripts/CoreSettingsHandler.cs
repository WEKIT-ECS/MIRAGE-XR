using ReadyPlayerMe.Core.Data;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    public static class CoreSettingsHandler
    {
        private const string RESOURCE_PATH = "Settings/CoreSettings";
        public static CoreSettings CoreSettings
        {
            get
            {
                if (coreSettings != null) return coreSettings;
                coreSettings = Load();
                if (coreSettings == null)
                {
                    Debug.LogWarning("CoreSettings could not be loaded.");
                }
                return coreSettings;
            }
        }

        private static CoreSettings coreSettings;

        public static CoreSettings Load()
        {
            return Resources.Load<CoreSettings>(RESOURCE_PATH);
        }
    }
}

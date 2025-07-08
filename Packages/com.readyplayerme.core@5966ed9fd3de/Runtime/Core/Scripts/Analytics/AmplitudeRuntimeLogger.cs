using System.Collections.Generic;

namespace ReadyPlayerMe.Core.Analytics
{
    /// <summary>
    /// Runtime analytics for sample which runs in the editor only.
    /// </summary>
    public class AmplitudeRuntimeLogger : IAnalyticsRuntimeLogger
    {
        private const string RUN_QUICKSTART_SCENE = "run quickstart scene";
        private const string PERSONAL_AVATAR_LOADING = "personal avatar loading";
        private const string AVATAR_URL = "avatar url";
        private const string PERSONAL_AVATAR_LOAD_BUTTON_CLICKED = "personal avatar load button clicked";

        public void LogRunQuickStartScene()
        {
            LogEvent(RUN_QUICKSTART_SCENE);
        }

        public void LogPersonalAvatarLoading(string url)
        {
            LogEvent(PERSONAL_AVATAR_LOADING, new Dictionary<string, object>
            {
                { AVATAR_URL, url }
            });
        }

        public void LogLoadPersonalAvatarButton()
        {
            LogEvent(PERSONAL_AVATAR_LOAD_BUTTON_CLICKED);
        }

        private void LogEvent(string eventName, Dictionary<string, object> eventProperties = null, Dictionary<string, object> userProperties = null)
        {
            if (!CoreSettingsHandler.CoreSettings.EnableAnalytics) return;

#if UNITY_EDITOR
            AmplitudeEventLogger.LogEvent(eventName, eventProperties, userProperties);
#endif
        }
    }
}

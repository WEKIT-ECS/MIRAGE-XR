using ReadyPlayerMe.Core.Editor;

namespace ReadyPlayerMe.Core.Analytics
{
    public static class AnalyticsEditorLogger
    {
        public static readonly IAnalyticsEditorLogger EventLogger;

        static AnalyticsEditorLogger()
        {
            IsEnabled = CoreSettingsHandler.CoreSettings.EnableAnalytics;
            EventLogger = new AmplitudeEditorLogger(IsEnabled);
        }

        public static bool IsEnabled { get; private set; }

        public static void Enable()
        {
            IsEnabled = true;
            EventLogger.Enable();
            CoreSettingsSetter.SetEnableAnalytics(true);
        }

        public static void Disable()
        {
            EventLogger.Disable();
            IsEnabled = false;
            CoreSettingsSetter.SetEnableAnalytics(false);
        }
    }
}

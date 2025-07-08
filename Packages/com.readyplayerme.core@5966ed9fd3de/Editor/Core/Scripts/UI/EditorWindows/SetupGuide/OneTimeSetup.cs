using ReadyPlayerMe.Core.Analytics;
using UnityEditor;

namespace ReadyPlayerMe.Core.Editor
{
    [InitializeOnLoad]
    public static class OneTimeSetup
    {
        static OneTimeSetup()
        {
            if (!ProjectPrefs.GetBool(ProjectPrefs.FIRST_TIME_SETUP_DONE))
            {
                EditorApplication.update += OnStartup;
            }
        }

        private static void OnStartup()
        {
            EditorApplication.update -= OnStartup;
            AnalyticsEditorLogger.Enable();
            SetupGuide.ShowWindow();
            ProjectPrefs.SetBool(ProjectPrefs.FIRST_TIME_SETUP_DONE, true);
        }

        private static void OnQuit()
        {
            AnalyticsEditorLogger.EventLogger.LogCloseProject();
        }

        private static bool CanShowWindow()
        {
            return !ProjectPrefs.GetBool(ProjectPrefs.FIRST_TIME_SETUP_DONE);
        }
    }
}

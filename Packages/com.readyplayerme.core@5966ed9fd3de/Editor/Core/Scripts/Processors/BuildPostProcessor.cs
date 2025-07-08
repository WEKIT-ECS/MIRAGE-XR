using ReadyPlayerMe.Core.Analytics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ReadyPlayerMe.Core.Editor
{
    public static class BuildPostProcessor
    {
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            // create asset if it has been deleted
            CoreSettingsLoader.EnsureSettingsExist();
            AppData appData = ApplicationData.GetData();
            AnalyticsEditorLogger.EventLogger.LogBuildApplication(appData.BuildTarget, PlayerSettings.productName, !Debug.isDebugBuild);
        }
    }
}

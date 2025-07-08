using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ReadyPlayerMe.Core
{
    public static class ApplicationData
    {
        public const string SDK_VERSION = "v7.3.1";
        private const string TAG = "ApplicationData";
        private const string DEFAULT_RENDER_PIPELINE = "Built-In Render Pipeline";
        private static readonly AppData Data;

        static ApplicationData()
        {
            Data.SDKVersion = SDK_VERSION;
            Data.PartnerName = CoreSettingsHandler.CoreSettings.Subdomain;
            Data.UnityVersion = Application.unityVersion;
            Data.UnityPlatform = Application.platform.ToString();
            Data.RenderPipeline = GetRenderPipeline();
#if UNITY_EDITOR
            Data.BuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
#endif
        }

        private static string GetRenderPipeline()
        {
            var renderPipeline = GraphicsSettings.currentRenderPipeline == null
                ? DEFAULT_RENDER_PIPELINE
                : GraphicsSettings.currentRenderPipeline.name;
            return renderPipeline;
        }

        public static void Log()
        {
            SDKLogger.Log(TAG, $"Partner Subdomain: <color=green>{Data.PartnerName}</color>");
            SDKLogger.Log(TAG, $"Unity Version: <color=green>{Data.UnityVersion}</color>");
            SDKLogger.Log(TAG, $"Unity Platform: <color=green>{Data.UnityPlatform}</color>");
            SDKLogger.Log(TAG, $"Unity Render Pipeline: <color=green>{Data.RenderPipeline}</color>");
#if UNITY_EDITOR
            SDKLogger.Log(TAG, $"Unity Build Target: <color=green>{Data.BuildTarget}</color>");
#endif
        }

        public static AppData GetData()
        {
            return Data;
        }
    }
}

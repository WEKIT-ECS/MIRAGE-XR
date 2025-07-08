using System.Runtime.InteropServices;

namespace ReadyPlayerMe.Samples.WebGLSample
{
    public static class WebInterface
    {

        [DllImport("__Internal")]
        private static extern void SetupRpm(string partner, string targetGameObjectName = "");

        [DllImport("__Internal")]
        private static extern void ShowReadyPlayerMeFrame();

        [DllImport("__Internal")]
        private static extern void HideReadyPlayerMeFrame();

        [DllImport("__Internal")]
        private static extern void ReloadUrl(string url);

        public static void SetIFrameVisibility(bool isVisible)
        {
            if (isVisible)
            {
                ShowReadyPlayerMeFrame();
                return;
            }
            HideReadyPlayerMeFrame();

        }

        public static void SetupRpmFrame(string url, string targetGameObjectName)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
    SetupRpm(url,  targetGameObjectName);
#endif
        }

        public static void ReloadWithUrl(string url)
        {
            ReloadUrl(url);
        }
    }
}

using System;
using UnityEngine;

#if UNITY_VISIONOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace PolySpatial.Samples
{
    public class SwiftFPSCounter : MonoBehaviour
    {
        [SerializeField]
        float m_RefreshPeriod = 0.75f;

        float m_LastRefreshTime;
        int m_LastRefreshFrame;

        void OnDisable()
        {
            m_LastRefreshTime = 0;
        }

        void Update()
        {
            var unscaledTime = Time.unscaledTime;
            if (unscaledTime - m_RefreshPeriod < m_LastRefreshTime)
                return;

            var currentFrame = Time.frameCount;
            var elapsedTime = unscaledTime - m_LastRefreshTime;
            var elapsedFrames = currentFrame - m_LastRefreshFrame;
            SetFPS(elapsedFrames / elapsedTime);

            m_LastRefreshTime = unscaledTime;
            m_LastRefreshFrame = currentFrame;
        }

#if UNITY_VISIONOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void SetFPS(float fps);
#else
        static void SetFPS(float _) { }
#endif
    }
}

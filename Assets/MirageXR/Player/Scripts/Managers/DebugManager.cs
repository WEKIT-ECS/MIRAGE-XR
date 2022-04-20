using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class DebugManager : MonoBehaviour
    {
        [Tooltip ("Drag debug text object here.")]
        [SerializeField] private Text DebugText;

        [Tooltip ("Drag device info text here.")]
        [SerializeField] private Text DeviceInfo;

        private void OnEnable ()
        {
            // Register to event manager events.
            EventManager.OnDebugLog += DebugLog;
        }

        private void OnDisable ()
        {
            // Unregister from event manager events.
            EventManager.OnDebugLog -= DebugLog;
        }

        // Use this for initialization.
        private void Start ()
        {
            if (DebugText == null)
            {
                Debug.Log ("Debug manager error: Debug text not found. Please add one in editor.");
            }

            if (DeviceInfo == null)
            {
                Debug.Log ("Debug manager error: Device info text not found. Please add one in editor.");
            }

            DeviceInfo.text = SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        /// Add debug message to debug console.
        /// </summary>
        /// <param name="debug">Debug message.</param>
        private void DebugLog (string debug)
        {
            DebugText.text = debug + "\n" + DebugText.text;
        }

        /// <summary>
        /// Clear debug console. Called from UiManager.
        /// </summary>
        private void ClearDebug ()
        {
            DebugText.text = "";
        }
    }
}

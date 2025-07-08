using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.Core
{
    public class RuntimeDebugPanel : MonoBehaviour
    {
        private bool showDebugPanel = true;
        private bool pauseLogOutput;
        private Text logTextUI;
        private ScrollRect logScrollRect;
        private string currentLogOutput = "<color=green>Log Output Started...</color>\n";
        private int logCount;
        private readonly int maxLogs = 200;

        public void ToggleShowDebugPanel()
        {
            showDebugPanel = !showDebugPanel;
        }

        public void TogglePauseLogOutput()
        {
            pauseLogOutput = !pauseLogOutput;
        }

        private void Awake()
        {
            InitialiseDebugPanel();
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void InitialiseDebugPanel()
        {
            logScrollRect = GetComponentInChildren<ScrollRect>(true);
            logTextUI = GetComponentInChildren<Text>(true);
            UpdateDebugPanel();
        }

        private void UpdateDebugPanel()
        {
            if (logTextUI) logTextUI.text = currentLogOutput;
            logCount++;
            ScrollToBottom();
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (logCount <= maxLogs)
            {
                currentLogOutput += $"{logString}\n";
            }
            else
            {
                currentLogOutput = "<color=yellow>Maximum number of logs reached. Logging Reset.</color>\n";
                logCount = 0;
            }

            if (showDebugPanel && !pauseLogOutput) UpdateDebugPanel();
        }

        private void ScrollToBottom()
        {
            if (logScrollRect) logScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}

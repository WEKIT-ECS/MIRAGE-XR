namespace CSE.MRTK.Toolkit.DebugConsole
{
    using MRTKUtilities.Application;
    using UnityEngine;

    /// <summary>
    /// Log to the <see cref="TMPro.TextMeshProUGUI"/> script to show the message in the UI.
    /// </summary>
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class UILogger : ControllerSubscriber
    {
        private TMPro.TextMeshProUGUI _content = null;

        /// <inheritdoc/>
        protected override void OnMessageAdded(string message)
        {
            if (_content == null)
            {
                _content = GetComponent<TMPro.TextMeshProUGUI>();
            }

            // add to the UI
            UnityDispatcher.InvokeOnAppThread(() => _content.text += $"{message}\n");
        }

        /// <inheritdoc/>
        protected override void OnClearLog()
        {
            if (_content == null)
            {
                _content = GetComponent<TMPro.TextMeshProUGUI>();
            }
            _content.text = string.Empty;
        }

        /// <inheritdoc/>
        protected override void OnSettingsUpdated(Settings settings)
        {
            // ignore.
        }
    }
}

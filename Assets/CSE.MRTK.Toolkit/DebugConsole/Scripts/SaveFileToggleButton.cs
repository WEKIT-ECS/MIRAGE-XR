namespace CSE.MRTK.Toolkit.DebugConsole
{
    using Microsoft.MixedReality.Toolkit.UI;
    using UnityEngine;

    /// <summary>
    /// Toggle button handler class for enable/disable saving messages to disk.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class SaveFileToggleButton : ControllerSubscriber
    {
        private Interactable _interactable = null;

        /// <inheritdoc/>
        protected override void OnSettingsUpdated(Settings settings)
        {
            if (_interactable == null)
            {
                _interactable = GetComponent<Interactable>();
            }

            // if setting changed, show in UI.
            if (_interactable.IsToggled != settings.SaveToFile)
            {
                _interactable.IsToggled = settings.SaveToFile;
            }
        }

        /// <inheritdoc/>
        protected override void OnMessageAdded(string message)
        {
            // ignore.
        }

        /// <inheritdoc/>
        protected override void OnClearLog()
        {
            // ignore.
        }

        /// <summary>
        /// Handle click of the toggle button and store state.
        /// </summary>
        public void OnClick()
        {
            if (Manager.Settings.SaveToFile != _interactable.IsToggled)
            {
                if (_interactable.IsToggled)
                {
                    // show this in the message, not in the log
                    Controller.WriteMessage($"*** START SAVING TO FILE FROM HERE ***");
                }

                Manager.Settings.SaveToFile = _interactable.IsToggled;
                Manager.SaveSettings();

                if (!_interactable.IsToggled)
                {
                    // show this in the message, not in the log
                    Controller.WriteMessage($"*** SAVING TO FILE STOPPED HERE ***");
                }
            }
        }
    }
}

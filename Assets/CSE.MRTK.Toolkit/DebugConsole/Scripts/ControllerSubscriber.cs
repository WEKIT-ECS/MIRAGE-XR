namespace CSE.MRTK.Toolkit.DebugConsole
{
    using UnityEngine;

    /// <summary>
    /// Class implementing subscribing to <see cref="UIController"/> 
    /// and <see cref="SettingsManager"/> events.
    /// Just inherit this class and implement abstract methods.
    /// </summary>
    public abstract class ControllerSubscriber : MonoBehaviour
    {
        /// <summary>
        /// Gets the UI controller.
        /// </summary>
        private MainController _controller;
        public MainController Controller => _controller;

        /// <summary>
        /// Gets the settings manager.
        /// </summary>
        private SettingsManager _manager;
        public SettingsManager Manager => _manager;

        /// <summary>
        /// Called when game object is enabled.
        /// </summary>
        private void OnEnable()
        {
            if (_controller == null)
            {
                _controller = GetComponentInParent<MainController>();
            }

            if (_controller != null)
            {
                _controller.OnMessage += OnMessageAdded;
                _controller.OnClearLog += OnClearLog;
            }

            if (_manager == null)
            {
                _manager = GetComponentInParent<SettingsManager>();
            }

            if (_manager != null)
            {
                OnSettingsUpdated(_manager.Settings);
                _manager.OnSettingsUpdated += OnSettingsUpdated;
            }
        }

        /// <summary>
        /// Called when game object is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.OnMessage -= OnMessageAdded;
            }

            if (_manager != null)
            {
                _manager.OnSettingsUpdated -= OnSettingsUpdated;
            }
        }

        protected abstract void OnSettingsUpdated(Settings settings);

        protected abstract void OnMessageAdded(string message);

        protected abstract void OnClearLog();
    }
}

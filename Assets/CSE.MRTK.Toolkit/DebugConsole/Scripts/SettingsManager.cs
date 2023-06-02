namespace CSE.MRTK.Toolkit.DebugConsole
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using UnityEngine;
#if WINDOWS_UWP
    using Windows.Storage;
#endif

    /// <summary>
    /// Manager of the settings that are stored on and restored from disk.
    /// </summary>
    [RequireComponent(typeof(DebugConsole.MainController))]
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Enter the name of the state file.")]
        private string _stateFileName = "debug-console-state.json";

        /// <summary>
        /// Gets the settings.
        /// </summary>
        private Settings _settings = new Settings();
        public Settings Settings => _settings;

        private string _statePath;

        /// <summary>
        /// Event fired when settings are changed (from file).
        /// </summary>
        /// <param name="settings">Changed settings.</param>
        public delegate void SettingsUpdated(Settings settings);
        public event SettingsUpdated OnSettingsUpdated;

        private bool _onlyLogWhenEnabled;
        public bool OnlyLogWhenEnabled => _onlyLogWhenEnabled;

        /// <summary>
        /// Initialize when enabled.
        /// </summary>
        private void OnEnable()
        {
            MainController controller = GetComponent<MainController>();
            _onlyLogWhenEnabled = controller.OnlyLogWhenEnabled;
#if WINDOWS_UWP
            _statePath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, _stateFileName);
#else
            _statePath = Path.Combine(Application.persistentDataPath, _stateFileName);
#endif
            LoadSettings();
        }

        /// <summary>
        /// Save the settings for the debug console.
        /// </summary>
        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(_statePath, json);
            OnSettingsUpdated?.Invoke(_settings);
        }

        /// <summary>
        /// Read the settings for the debug console.
        /// </summary>
        private void LoadSettings()
        {
            if (!File.Exists(_statePath))
            {
                _settings = new Settings();
            }
            else
            {
                try
                {
                    string json = File.ReadAllText(_statePath);
                    _settings = JsonConvert.DeserializeObject<Settings>(json);
                }
                catch (Exception ex)
                {
                    _settings = new Settings();
                    Debug.LogError($"DebugConsoleStateManager.ReadState ERROR: {ex}");
                }
            }

            OnSettingsUpdated?.Invoke(_settings);
        }

    }
}

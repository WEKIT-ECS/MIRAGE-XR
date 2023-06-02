namespace MRTKUtilities.Application
{
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.UI;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// This is the main manager of the application. It loads the app settings
    /// and gets an access token for the logged in user. The access token
    /// can then be used for backend API access.
    /// </summary>
    [RequireComponent(typeof(ApiClient))]
    public class ApplicationManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="ApplicationManager"/>. The rest
        /// of the code in the application can just use ApplicationManager.Instance to get
        /// settings, the <see cref="ApiClient"/>.
        /// </summary>
        public static ApplicationManager Instance { get; private set; }

        /// <summary>
        /// Sets the property in the Unity Editor for a message dialog that can
        /// be used for showing messages and errors to the user.
        /// Other parts of the application can reuse this dialog like this:
        /// <example>
        /// <code>
        /// Dialog.Open(ApplicationMaanger.Instance.DialogPrefab.gameObject, DialogButtonType.OK, "Dialog title", "This is the message!", true);
        /// </code>
        /// </example>
        /// </summary>
        [Tooltip("Assign a dialog prefab to be used for messages.")]
        [SerializeField]
        private DialogShell _dialogPrefab;
        public DialogShell DialogPrefab => _dialogPrefab;

        /// <summary>
        /// Event fired when application manager is initialized.
        /// </summary>
        public delegate void Initialized();
        public event Initialized OnInitialized;

        /// <summary>
        /// Gets or sets the flag wether the application manager is initialized.
        /// </summary>
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Gets or sets the application settings with secrets. Read from internal file.
        /// </summary>
        private SettingsManager _settingsManager;
        public SettingsManager SettingsManager => _settingsManager;

        /// <summary>
        /// Gets or sets the authentication manager.
        /// </summary>
        private AuthenticationManager _authManager;
        public AuthenticationManager AuthenticationManager => _authManager;

        /// <summary>
        /// Gets the data client for backend API access.
        /// </summary>
        private ApiClient _apiClient;
        public ApiClient ApiClient => _apiClient;

        /// <summary>
        /// Refresh local cache. Remove everything and re-read the projects-labsession
        /// hierarchy. Rest will flow in automatically.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshCacheAsync()
        {
            LocalCacheHelper.ClearCache();

            // TODO: refresh other cached data
        }

        /// <summary>
        /// On start of the game object, which should be in the main scene to launch on startup.
        /// </summary>
        private async void Start()
        {
            _isInitialized = false;

            // make application manager available through static
            Instance = this;

            // switch off the profiler by default
            CoreServices.DiagnosticsSystem.ShowProfiler = false;

            _settingsManager = GetComponent<SettingsManager>();
            _authManager = GetComponent<AuthenticationManager>();
            _apiClient = GetComponent<ApiClient>();

            if (_dialogPrefab == null)
            {
                Debug.LogError($"ERROR ApplicationManager.Start: no message dialog prefab is provided for user errors.");
            }

            if (SettingsManager.LoadSettings())
            {
                // authenticate user
                string token = await AuthenticationManager.AuthenticateUserAsync();

                // NOTE: Use this if you want to (re)discover the redirect URL for this app
                // based on the PackageId of the UWP app.
                // Debug.Log($"RedirecT URL: {AuthenticationHelper.GetRedirectUrl()}");

                if (!string.IsNullOrEmpty(token))
                {
                    // setup local cache for determined internId for the user.
                    LocalCacheHelper.Initialize(AuthenticationManager.ObjectId);

                    // load initial data
                    await ApiClient.InitializeAsync();

                    // register for data changes in memory
                    ApiClient.OnDataUpdated += ApiClient_DataUpdated;

                    // Let others know the application is initialized
                    _isInitialized = true;
                    OnInitialized?.Invoke();
                }
                else
                {
                    Debug.Log($"ApplicationManager.Start: token couldn't be retrieved. No authentication possible.");
                }
            }
            else
            {
                Debug.Log("ApplicationManager.Start: appsettings were not found or are invalid. No extra functionality is available.");
            }

            // Let others know we've initialized
            _isInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Handle change data in ApiClient. We'll store it in the local cache.
        /// </summary>
        private void ApiClient_DataUpdated()
        {
            LocalCacheHelper.StoreData(ApiClient.Data);
        }

        /// <summary>
        /// Example method for showing a message dialog using the <see cref="DialogPrefab"/>.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message to show in the dialog.</param>
        private void ShowDialog(string title, string message)
        {
            Debug.LogError(message);
            if (_dialogPrefab != null)
            {
                Dialog.Open(_dialogPrefab.gameObject, DialogButtonType.OK, title, message, true);
            }
        }
    }
}
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

namespace MirageXR
{
    public class ProfileScreenSpatialViewController : ScreenViewController<ProfileScreenSpatialViewController, ProfileScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.ProfileScreen;

        private SketchfabUserInfo _userInfo;
        private bool _isSketchfabLoggedIn;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonRegisterClick(OnButtonRegisterClicked);
            View.SetActionOnButtonLoginClick(OnButtonLoginClicked);
            View.SetActionOnButtonAudioDeviceClick(ShowAudioDeviceView);
            View.SetActionOnButtonSketchfabClick(OnSketchfabSignInButtonClicked);
            View.SetActionOnButtonAvatarClick(ShowChangeUserAvatarView);
            View.SetActionOnToggleConsoleValueChanged(OnToggleConsoleValueChanged);

            View.SetActionOnButtonServerAddressClick(ShowChangeServerAddressPanel);
            View.SetActionOnButtonServerPortClick(ShowChangeServerPortPanel);
            View.SetActionOnButtonWebsocketAddressClick(ShowChangeWebsocketAddressPanel);
            View.SetActionOnButtonWebsocketPortClick(ShowChangeWebsocketPortPanel);

            View.gameObject.SetActive(false);

            var sketchfabManager = RootObject.Instance.LEE.SketchfabManager;
            sketchfabManager.OnSketchfabUserDataChanged += OnSketchfabUserDataChanged;
            sketchfabManager.OnSketchfabLoggedIn += OnSketchfabLoggedIn;

            var configManager = RootObject.Instance.LEE.ConfigManager;
            configManager.OnNetworkServerAddressChanged += OnNetworkServerAddressChanged;
            configManager.OnNetworkServerPortChanged += OnNetworkServerPortChanged;
            configManager.OnNetworkWebsocketAddressChanged += OnNetworkWebsocketAddressChanged;
            configManager.OnNetworkWebsocketPortChanged += OnNetworkWebsocketPortChanged;
        }

        private void OnToggleConsoleValueChanged(bool value)
        {
            if (value)
            {
                MenuManager.Instance.ShowInGameConsole();
            }
            else
            {
                MenuManager.Instance.HideInGameConsole();
            }
        }

        private void OnSketchfabUserDataChanged(SketchfabUserInfo userInfo)
        {
            _userInfo = userInfo;
            if (_isSketchfabLoggedIn)
            {
                View.SetSketchfabText($"Logged In as {_userInfo.Username}");
            }
        }

        private void OnSketchfabLoggedIn(bool value)
        {
            _isSketchfabLoggedIn = value;
            if (value)
            {
                var text = _userInfo == null ? "Logged In" : $"Logged In as {_userInfo.Username}";
                View.SetSketchfabText(text);
            }
            else
            {
                _userInfo = null;
            }
        }

        private void OnSketchfabSignInButtonClicked()
        {
            var prefab = View.GetSketchfabSignInPopupViewPrefab();
            if (prefab is not null)
            {
                PopupsViewer.Instance.Show(prefab);
            }
        }

        private void ShowAudioDeviceView()
        {
            var prefab = View.GetAudioDevicePrefab();
            if (prefab is not null)
            {
                PopupsViewer.Instance.Show(prefab);
            }
        }

		private void ShowChangeUserAvatarView()
		{
            var prefab = View.GetChangeUserAvatarViewPrefab();
            if (prefab is not null)
            {
                PopupsViewer.Instance.Show(prefab);
            }
		}

		private void OnButtonLoginClicked()
        {
            // TODO
            //MenuManager.Instance.ShowSignInView();
            //View.ShowSignInPanel();
            OnOidcLogin(); //temp
        }

        private void OnOidcLogin()
        {
            RootObject.Instance.LEE.AuthorizationManager.OnLoginCompleted += OnOidcLoginCompleted;
            LearningExperienceEngine.LearningExperienceEngine.Instance.AuthorizationManager.Login().Forget();
        }

        private void OnOidcLoginCompleted(string accessToken)
        {
            RootObject.Instance.LEE.AuthorizationManager.OnLoginCompleted -= OnOidcLoginCompleted;
            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
            MenuManager.Instance.ShowScreen(ScreenName.MainScreen);
        }

        private void OnButtonRegisterClicked()
        {
            //View.ShowRegisterPanel(); // TODO: replace with RegisterScreenView.prefab (but now we use url, see OnClickRegister())
        }

        private void OnNetworkServerAddressChanged(string address)
        {
            View.SetServerAddressText(address);
        }

        private void OnNetworkServerPortChanged(string port)
        {
            View.SetServerPortText(port);
        }

        private void OnNetworkWebsocketAddressChanged(string address)
        {
            View.SetWebsocketAddressText(address);
        }

        private void OnNetworkWebsocketPortChanged(string port)
        {
            View.SetWebsocketPortText(port);
        }

        private static bool IsValidPort(string port)
        {
            return int.TryParse(port, out _);
        }

        private static bool IsValidUrl(string urlString)
        {
            const string regexExpression = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            var regex = new Regex(regexExpression);
            return regex.IsMatch(urlString);
        }

        private static bool IsValidWebsocketUrl(string urlString)
        {
            const string regexExpression = @"^(?:ws(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            var regex = new Regex(regexExpression);
            return regex.IsMatch(urlString);
        }

        private static void ShowChangeServerAddressPanel()
        {
            const string other = "Other";
            const string label = "Server address:";
            
            var configManager = RootObject.Instance.LEE.ConfigManager;
            var defaultConfig = configManager.GetDefaultNetworkServerAddress();
            var currentConfig = configManager.GetNetworkServerAddress();

            var isDefault = currentConfig == defaultConfig;

            if (isDefault)
            {
                MenuManager.Instance.Dialog.ShowBottomMultilineToggles(label,
                    (defaultConfig, null, false, true),
                    (other, ShowServerAddressPanel, false, false)
                );
            }
            else
            {
                MenuManager.Instance.Dialog.ShowBottomMultilineToggles(label,
                    (defaultConfig, ResetNetworkServerAddress, false, false),
                    (currentConfig, null, false, true),
                    (other, ShowServerAddressPanel, false, false)
                );
            }
        }

        private static void ResetNetworkServerAddress()
        {
            RootObject.Instance.LEE.ConfigManager.ResetNetworkServerAddress();
            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
        }

        private static void ShowServerAddressPanel()
        {
            MenuManager.Instance.Dialog.ShowBottomInputField(
                "Custom server address:",
                "Enter address",
                "Cancel", null,
                "Save", ChangeServerAddress);
        }

        private static void ChangeServerAddress(string address)
        {
            if (!IsValidUrl(address))
            {
                Toast.Instance.Show("Server address is invalid!");
                return;
            }

            RootObject.Instance.LEE.ConfigManager.SetNetworkServerAddress(address);
            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
        }

        private static void ShowChangeServerPortPanel()
        {
            const string other = "Other";
            const string label = "Server port:";

            var configManager = RootObject.Instance.LEE.ConfigManager;
            var defaultConfig = configManager.GetDefaultNetworkServerPort();
            var currentConfig = configManager.GetNetworkServerPort();

            var isDefault = currentConfig == defaultConfig;

            if (isDefault)
            {
                MenuManager.Instance.Dialog.ShowBottomMultilineToggles(label,
                    (defaultConfig, null, false, true),
                    (other, ShowServerPortPanel, false, false)
                );
            }
            else
            {
                MenuManager.Instance.Dialog.ShowBottomMultilineToggles(label,
                    (defaultConfig, ResetNetworkServerPort, false, false),
                    (currentConfig, null, false, true),
                    (other, ShowServerPortPanel, false, false)
                );
            }
        }

        private static void ResetNetworkServerPort()
        {
            RootObject.Instance.LEE.ConfigManager.ResetNetworkServerPort();
            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
        }

        private static void ShowServerPortPanel()
        {
            MenuManager.Instance.Dialog.ShowBottomInputField(
                "Custom server port:",
                "Enter port",
                "Cancel", null,
                "Save", ChangeServerPort);
        }

        private static void ChangeServerPort(string port)
        {
            if (!IsValidPort(port))
            {
                Toast.Instance.Show("Server port is invalid!");
                return;
            }

            RootObject.Instance.LEE.ConfigManager.SetNetworkServerPort(port);
            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
        }

        private static void ShowChangeWebsocketAddressPanel()
        {
            const string other = "Other";
            const string label = "Websocket address:";
            
            var configManager = RootObject.Instance.LEE.ConfigManager;
            var defaultConfig = configManager.GetDefaultNetworkWebsocketAddress();
            var currentConfig = configManager.GetNetworkWebsocketAddress();

            var isDefault = currentConfig == defaultConfig;

            if (isDefault)
            {
                MenuManager.Instance.Dialog.ShowBottomMultilineToggles(label,
                    (defaultConfig, null, false, true),
                    (other, ShowWebsocketAddressPanel, false, false)
                );
            }
            else
            {
                MenuManager.Instance.Dialog.ShowBottomMultilineToggles(label,
                    (defaultConfig, ResetNetworkWebsocketAddress, false, false),
                    (currentConfig, null, false, true),
                    (other, ShowWebsocketAddressPanel, false, false)
                );
            }
        }

        private static void ResetNetworkWebsocketAddress()
        {
            RootObject.Instance.LEE.ConfigManager.ResetNetworkWebsocketAddress();
        }

        private static void ShowWebsocketAddressPanel()
        {
            MenuManager.Instance.Dialog.ShowBottomInputField(
                "Custom websocket address:",
                "Enter address",
                "Cancel", null,
                "Save", ChangeWebsocketAddress);
        }

        private static void ChangeWebsocketAddress(string address)
        {
            if (!IsValidWebsocketUrl(address))
            {
                Toast.Instance.Show("Websocket address is invalid!");
                return;
            }

            RootObject.Instance.LEE.ConfigManager.SetNetworkWebsocketAddress(address);
        }

        private static void ShowChangeWebsocketPortPanel()
        {
            const string other = "Other";
            const string label = "Websocket port:";

            var configManager = RootObject.Instance.LEE.ConfigManager;
            var defaultConfig = configManager.GetDefaultNetworkWebsocketPort();
            var currentConfig = configManager.GetNetworkWebsocketPort();

            var isDefault = currentConfig == defaultConfig;

            if (isDefault)
            {
                MenuManager.Instance.Dialog.ShowBottomMultilineToggles(label,
                    (defaultConfig, null, false, true),
                    (other, ShowWebsocketPortPanel, false, false)
                );
            }
            else
            {
                MenuManager.Instance.Dialog.ShowBottomMultilineToggles(label,
                    (defaultConfig, ResetNetworkWebsocketPort, false, false),
                    (currentConfig, null, false, true),
                    (other, ShowWebsocketPortPanel, false, false)
                );
            }
        }

        private static void ResetNetworkWebsocketPort()
        {
            RootObject.Instance.LEE.ConfigManager.ResetNetworkWebsocketPort();
        }

        private static void ShowWebsocketPortPanel()
        {
            MenuManager.Instance.Dialog.ShowBottomInputField(
                "Custom server port:",
                "Enter port",
                "Cancel", null,
                "Save", ChangeWebsocketPort);
        }

        private static void ChangeWebsocketPort(string port)
        {
            if (!IsValidPort(port))
            {
                Toast.Instance.Show("Websocket port is invalid!");
                return;
            }

            RootObject.Instance.LEE.ConfigManager.SetNetworkWebsocketPort(port);
        }
    }
}

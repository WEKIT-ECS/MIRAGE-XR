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
            View.gameObject.SetActive(false);

            RootObject.Instance.LEE.SketchfabManager.OnSketchfabUserDataChanged += OnSketchfabUserDataChanged;
            RootObject.Instance.LEE.SketchfabManager.OnSketchfabLoggedIn += OnSketchfabLoggedIn;
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
    }
}

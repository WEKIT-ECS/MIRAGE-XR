using LearningExperienceEngine;

namespace MirageXR
{
    public class ProfileScreenSpatialViewController : ScreenViewController<ProfileScreenSpatialViewController, ProfileScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.ProfileScreen;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonRegisterClick(OnButtonRegisterClicked);
            View.SetActionOnButtonLoginClick(OnButtonLoginClicked);
            View.SetActionOnButtonAudioDeviceClick(ShowAudioDeviceView);
            View.SetActionOnButtonSketchfabClick(OnSketchfabSignInButtonClicked);
            View.gameObject.SetActive(false);

            RootObject.Instance.LEE.SketchfabManager.OnSketchfabLoggedIn += OnSketchfabLoggedIn;
        }

        private void OnSketchfabLoggedIn(bool value)
        {
            View.SetSketchfabText("Logged In");
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

        private void OnButtonLoginClicked()
        {
            // TODO
            //MenuManager.Instance.ShowSignInView();
            //View.ShowSignInPanel();
            OnOidcLogin(); //temp
        }

        private void OnOidcLogin()
        {
            AuthManager.OnLoginCompleted += OnOidcLoginCompleted;
            LearningExperienceEngine.LearningExperienceEngine.Instance.authManager.Login();
        }

        private void OnOidcLoginCompleted(string accessToken)
        {
            AuthManager.OnLoginCompleted -= OnOidcLoginCompleted;

            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
            MenuManager.Instance.ShowScreen(ScreenName.MainScreen);
        }

        private void OnButtonRegisterClicked()
        {
            //View.ShowRegisterPanel(); // TODO: replace with RegisterScreenView.prefab (but now we use url, see OnClickRegister())
        }
    }
}

using LearningExperienceEngine;
using System;
using Cysharp.Threading.Tasks;

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
            View.SetActionOnButtonAvatarClick(ShowChangeUserAvatarView);
            View.gameObject.SetActive(false);

            RootObject.Instance.LEE.SketchfabManager.OnSketchfabLoggedIn += OnSketchfabLoggedIn;
        }

		private void OnSketchfabLoggedIn(bool value)
        {
            if (value)
            {
                View.SetSketchfabText("Logged In");
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

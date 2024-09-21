using UnityEngine;
using Utility.UiKit.Runtime.MVC;

namespace MirageXR
{
    public class ProfileScreenSpatialViewController : ScreenViewController<ProfileScreenSpatialViewController, ProfileScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.ProfileScreen;

        [SerializeField] private SidebarView sidebarView; // Temp
        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonRegisterClick(OnButtonRegisterClicked);
            View.SetActionOnButtonLoginClick(OnButtonLoginClicked);
            View.gameObject.SetActive(false);
        }

        private void OnButtonLoginClicked()
        {
            // TODO
            //MenuManager.Instance.ShowSignInView();
            View.ShowSignInPanel();
        }
        
        private void OnButtonRegisterClicked()
        {
            //View.ShowRegisterPanel(); / TODO: replace with RegisterScreenView.prefab (but now we use url, see OnClickRegister())
        }
    }
}

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
            MenuManager.Instance.ShowSignInView();
        }
        
        private void OnButtonRegisterClicked()
        {
            // TODO
            MenuManager.Instance.ShowRegisterView();
        }
    }
}

using Utility.UiKit.Runtime.MVC;

namespace MirageXR
{
    public class MainScreenSpatialViewController : ScreenViewController<MainScreenSpatialViewController, MainScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.MainScreen;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonSortingClick(() => {MenuManager.Instance.ShowSortingView(); });
            View.SetActionOnButtonAddNewActivityClick(OnAddNewActivityClick);
        }

        private void OnAddNewActivityClick()
        {
            MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen);
        }
    }
}

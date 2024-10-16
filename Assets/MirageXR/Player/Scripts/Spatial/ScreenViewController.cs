using Utility.UiKit.Runtime.MVC;

namespace MirageXR
{
    public enum ScreenName
    {
        None,
        MainScreen,
        ProfileScreen,
        NewActivityScreen,
        SettingsScreen
    }
    public abstract class ScreenViewController<TScreenViewController, TScreenView> : ViewController<TScreenViewController, TScreenView>
        where TScreenViewController : ScreenViewController<TScreenViewController, TScreenView>
        where TScreenView : ScreenView
    {
        public abstract ScreenName ScreenName { get; }

        protected override void OnBind()
        {
            base.OnBind();
            
            MenuManager.ScreenChanged.AddListener(OnScreenChanged);
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            
            MenuManager.ScreenChanged.RemoveListener(OnScreenChanged);
        }
        
        protected virtual void OnScreenChanged(ScreenName screenName, string args)
        {
            SetViewVisible(screenName == ScreenName);
        }
    }
}

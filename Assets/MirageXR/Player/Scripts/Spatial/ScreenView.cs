namespace MirageXR
{
    public class ScreenView : Utility.UiKit.Runtime.MVC.View
    {
        protected override void Awake()
        {
            base.Awake();
            
            SetActiveLayers(false);
        }
    }
}
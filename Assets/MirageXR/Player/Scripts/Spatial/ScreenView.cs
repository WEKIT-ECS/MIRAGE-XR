using Utility.UiKit.Runtime.MVC;

namespace MirageXR
{
    public class ScreenView : View
    {
        protected override void Awake()
        {
            base.Awake();
            
            SetActiveLayers(false);
        }
    }
}

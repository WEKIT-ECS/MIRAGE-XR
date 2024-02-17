
namespace MirageXR
{
    public class MobileStepClickActionStepDetails : MobileStep
    {
        protected override void Init()
        {
            this.instructionText = "Tap here to give more detail to your new action step.";
            this.highlightedObject = RootView.Instance.ToggleView.gameObject;

            EventManager.ViewSelectorClicked += this.DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.ViewSelectorClicked -= this.DefaultExitEventListener;
        }
    }
}

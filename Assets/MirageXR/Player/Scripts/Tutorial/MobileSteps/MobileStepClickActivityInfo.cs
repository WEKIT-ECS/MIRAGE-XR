
namespace MirageXR
{
    public class MobileStepClickActivityInfo : MobileStep
    {
        protected override void Init()
        {
            this.instructionText = "Tap here to edit your new activity data.";
            this.highlightedObject = RootView.Instance.ToggleSteps.gameObject;

            EventManager.StepsSelectorClicked += this.DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.StepsSelectorClicked -= this.DefaultExitEventListener;
        }
    }
}

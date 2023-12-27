
namespace MirageXR
{
    public class MobileStepCreateActionStep : MobileStep
    {
        protected override void Init()
        {
            this.instructionText = "Your learning activity should consist of multiple steps called �Actions� to progressively guide your students. To create a new step, \"Tap\" this + button.";
            this.highlightedObject = RootView.Instance.stepsListView.BtnAddStep.gameObject;

            EventManager.OnActionCreated += this.ActionCreatedListener;
        }

        private void ActionCreatedListener(Action action)
        {
            ExitStep();
        }

        protected override void Detach()
        {
            EventManager.OnActionCreated -= this.ActionCreatedListener;
        }
    }
}

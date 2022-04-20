using UnityEngine;

namespace MirageXR
{
    public class MobileStepClickAddStepContent : MobileStep
    {
        protected override void Init()
        {
            this.instructionText = "To add augmentations to this step, press the highlighted button.";
            this.highlightedObject = RootView.Instance.contentListView.BtnAddContent.gameObject;
            EventManager.MobileAddStepContentPressed += this.DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.MobileAddStepContentPressed -= this.DefaultExitEventListener;
        }

    }
}

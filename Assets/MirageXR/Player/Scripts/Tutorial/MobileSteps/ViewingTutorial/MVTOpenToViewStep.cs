using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{

    public class MVTOpenToViewStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "multiline_button", message = "Second step.", position = TutorialModel.MessagePosition.Bottom });
            this.manager.MobileTutorial.Show(queue);

            EventManager.OnActivateAction += NextStepByLabelTriggerListener;
        }

        private void NextStepByLabelTriggerListener(string action)
        {
            ExitStep();
        }

        protected override void SecuredExitStep()
        {
            this.manager.NextStep();
        }

        protected override void SecuredCloseStep()
        {
            this.manager.MobileTutorial.Hide();
            //nothing
        }
    }
}

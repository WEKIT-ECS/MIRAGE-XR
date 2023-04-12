using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class MVTCalibrationGuideStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "calibration_guide_ok", message = "Third step.", position = TutorialModel.MessagePosition.Top });
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

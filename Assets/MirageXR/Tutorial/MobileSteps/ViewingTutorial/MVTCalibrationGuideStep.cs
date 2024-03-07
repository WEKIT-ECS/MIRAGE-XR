using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace MirageXR
{
    public class MVTCalibrationGuideStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            RootObject.Instance.calibrationManager.onCalibrationFinished.AddListener(this.ExitListener);

            var queue = new Queue<TutorialModel>();
            if (!DBManager.dontShowCalibrationGuide)
            {
                queue.Enqueue(new TutorialModel { id = "calibration_guide_ok", message = "This popup serves as a reminder on how to complete calibration. As this tutorial will explain it anyway, click Ok for now.", position = TutorialModel.MessagePosition.Top });
            }
            queue.Enqueue(new TutorialModel { id = "activity_calibration", message = "We begin calibration by clicking the button below...", position = TutorialModel.MessagePosition.Top });
            queue.Enqueue(new TutorialModel { id = "activity_calibrationWithMarker", message = "and selecting Start Calibration.", position = TutorialModel.MessagePosition.Top });
            this.manager.MobileTutorial.Show(queue);
        }

        private async void ExitListener()
        {
            await Task.Delay(500);
            ExitStep();
        }

        protected override void SecuredExitStep()
        {
            this.manager.NextStep();
            RootObject.Instance.calibrationManager.onCalibrationFinished.RemoveListener(this.DefaultExitEventListener);
        }

        protected override void SecuredCloseStep()
        {
            this.manager.MobileTutorial.Hide();
            RootObject.Instance.calibrationManager.onCalibrationFinished.RemoveListener(this.DefaultExitEventListener);
        }
    }
}

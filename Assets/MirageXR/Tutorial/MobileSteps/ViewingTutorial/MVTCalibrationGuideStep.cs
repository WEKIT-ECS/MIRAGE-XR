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
                queue.Enqueue(new TutorialModel { Id = "calibration_guide_ok", Message = "This popup serves as a reminder on how to complete calibration. As this tutorial will explain it anyway, click Ok for now.", Position = TutorialModel.MessagePosition.Top });
            }
            queue.Enqueue(new TutorialModel { Id = "activity_calibration", Message = "We begin calibration by clicking the tab above...", Position = TutorialModel.MessagePosition.Bottom });
            queue.Enqueue(new TutorialModel { Id = "activity_calibrationWithMarker", Message = "and selecting Start Calibration.", Position = TutorialModel.MessagePosition.Top });
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

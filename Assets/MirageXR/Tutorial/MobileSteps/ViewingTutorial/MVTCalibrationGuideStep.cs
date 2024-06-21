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

            var queue = new Queue<TutorialModelUI>();
            if (!DBManager.dontShowCalibrationGuide)
            {
                queue.Enqueue(new TutorialModelUI { Id = "calibration_guide_ok", Message = "This popup serves as a reminder on how to complete calibration. As this tutorial will explain it anyway, click Ok for now.", Position = TutorialModelUI.MessagePosition.Top });
            }
            queue.Enqueue(new TutorialModelUI { Id = "activity_calibration", Message = "We begin calibration by clicking the tab above...", Position = TutorialModelUI.MessagePosition.Bottom });
            queue.Enqueue(new TutorialModelUI { Id = "activity_calibrationWithMarker", Message = "and selecting Start Calibration.", Position = TutorialModelUI.MessagePosition.Top });
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

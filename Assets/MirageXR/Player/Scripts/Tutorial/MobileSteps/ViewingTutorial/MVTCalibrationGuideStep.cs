using System.Collections.Generic;
using UnityEngine.UI;

namespace MirageXR
{
    public class MVTCalibrationGuideStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            /*
            CalibrationGuideView cgv = RootView_v2.Instance.gameObject.transform.FindDeepChild("CalibrationGuide(Clone)").GetComponent<CalibrationGuideView>();
            Button target = cgv.BtnClose;
            target.onClick.AddListener(this.DefaultExitEventListener);
            */
            /*
            ActivityView_v2 av = RootView_v2.Instance.activityView;
            Button target = av.Tabs.Find("Calibration").FindDeepChild("Button").GetComponent<Button>();
            target.onClick.AddListener(this.DefaultExitEventListener);
            */

            RootObject.Instance.calibrationManager.onCalibrationFinished.AddListener(this.DefaultExitEventListener);

            var queue = new Queue<TutorialModel>();
            if (!DBManager.dontShowCalibrationGuide)
            {
                queue.Enqueue(new TutorialModel { id = "calibration_guide_ok", message = "This popup serves as a reminder on how to complete calibration. As this tutorial will explain it anyway, click Ok for now.", position = TutorialModel.MessagePosition.Top });
            }
            queue.Enqueue(new TutorialModel { id = "activity_calibrate", message = "We begin calibration by clicking the button below...", position = TutorialModel.MessagePosition.Top });
            queue.Enqueue(new TutorialModel { id = "dialog_bottom_multiline_0", message = "and selecting Start Calibra", position = TutorialModel.MessagePosition.Middle });
            this.manager.MobileTutorial.Show(queue);
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

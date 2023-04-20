using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{

    public class MVTStartCalibrationStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            ActivityView_v2 av = RootView_v2.Instance.activityView;
            Button target = av.Tabs.Find("Calibration").FindDeepChild("Button").GetComponent<Button>();
            target.onClick.AddListener(this.DefaultExitEventListener);

            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "activity_calibrate", message = "Click the 510 activity.", position = TutorialModel.MessagePosition.Top });
            this.manager.MobileTutorial.Show(queue);
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

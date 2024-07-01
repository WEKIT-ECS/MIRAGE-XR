using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace MirageXR
{
    public class MVTSwitchTabsStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            RootView_v2.Instance.activityView.BtnArrow.onClick.AddListener(this.ExitListener);

            var queue = new Queue<TutorialStepModelUI>();
            queue.Enqueue(new TutorialStepModelUI { Id = "activity_steps", Message = "Great, calibration is now complete. Let's move on to the activity's content. First, switch to the Steps tab.", Position = TutorialStepModelUI.MessagePosition.Bottom });
            queue.Enqueue(new TutorialStepModelUI { Id = "ui_toggle", Message = "This screen shows all the Steps, the content of an Activity. It serves as an indicator of your progress, which Step you are on. For now let's begin interacting with the first Step's content by lowering the UI.", Position = TutorialStepModelUI.MessagePosition.Bottom });
            this.manager.MobileTutorial.Show(queue);
        }

        private async void ExitListener()
        {
            await Task.Delay(100);
            ExitStep();
        }

        protected override void SecuredExitStep()
        {
            RootView_v2.Instance.activityView.BtnArrow.onClick.RemoveListener(this.ExitListener);
            this.manager.NextStep();
        }

        protected override void SecuredCloseStep()
        {
            RootView_v2.Instance.activityView.BtnArrow.onClick.RemoveListener(this.ExitListener);
            this.manager.MobileTutorial.Hide();
        }
    }
}

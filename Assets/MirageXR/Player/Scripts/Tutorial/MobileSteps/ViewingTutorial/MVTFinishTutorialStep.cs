using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace MirageXR
{
    public class MVTFinishTutorialStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            RootView_v2.Instance.activityView.BtnArrow.onClick.AddListener(this.ExitListener);

            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "ui_toggle", message = "This ends the Viewing Tutorial! You can press this button to reenter the UI and revisit some of the previous steps of this activity. Enjoy!", position = TutorialModel.MessagePosition.Middle });
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

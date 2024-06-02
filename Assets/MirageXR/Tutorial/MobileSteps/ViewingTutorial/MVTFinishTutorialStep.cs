using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace MirageXR
{
    public class MVTFinishTutorialStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            RootView_v2.Instance.activityView.BtnArrow.onClick.AddListener(this.DefaultExitEventListener);

            var queue = new Queue<TutorialModelUI>();
            queue.Enqueue(new TutorialModelUI { Id = "ui_toggle", Message = "This ends the Viewing TutorialUI! You can press this button to reenter the UI and revisit some of the previous steps of this activity. Enjoy!", Position = TutorialModelUI.MessagePosition.Middle });
            this.manager.MobileTutorial.Show(queue);
        }

        protected override void SecuredExitStep()
        {
            RootView_v2.Instance.activityView.BtnArrow.onClick.RemoveListener(this.DefaultExitEventListener);
            this.manager.NextStep();
        }

        protected override void SecuredCloseStep()
        {
            RootView_v2.Instance.activityView.BtnArrow.onClick.RemoveListener(this.DefaultExitEventListener);
            this.manager.MobileTutorial.Hide();
        }
    }
}

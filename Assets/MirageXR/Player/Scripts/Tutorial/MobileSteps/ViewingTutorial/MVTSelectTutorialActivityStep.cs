using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{

    public class MVTSelectTutorialActivityStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "", message = "Click the first activity.", position = TutorialModel.MessagePosition.Bottom });
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

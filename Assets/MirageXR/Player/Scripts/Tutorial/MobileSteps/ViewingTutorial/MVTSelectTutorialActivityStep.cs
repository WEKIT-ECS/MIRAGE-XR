using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{

    public class MVTSelectTutorialActivityStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            ActivityListView_v2 alv = RootView_v2.Instance.activityListView;
            // Here it is necessary that the Tutorial Activity is the first in the list
            ActivityListItem_v2 tutorialActivityCard = alv.GetComponentsInChildren<ActivityListItem_v2>()[0];
            string name = tutorialActivityCard.activityName;

            TutorialItem titem = tutorialActivityCard.gameObject.AddComponent(typeof(TutorialItem)) as TutorialItem;
            titem.SetId("tutorial_activity");
            titem.SetInteractableObject(tutorialActivityCard.gameObject);
            titem.SetDelay(1);

            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "tutorial_activity", message = "Welcome to the MirageXR Viewing Tutorial! To start, click on the specialy prepared Tutorial Activity highlighted on your screen.", position = TutorialModel.MessagePosition.Bottom });
            queue.Enqueue(new TutorialModel { id = "dialog_middle_multiline_1", message = "An activity can be opened in two modes: editing (to make changes to the app) and viewing (read-only). For this tutorial, click Open to View.", position = TutorialModel.MessagePosition.Bottom });
            this.manager.MobileTutorial.Show(queue);

            // Next Step triggered by first step being activated
            EventManager.OnActivateAction += NextStepByLabelTriggerListener;
        }

        private void NextStepByLabelTriggerListener(string action)
        {
            ExitStep();
        }

        protected override void SecuredExitStep()
        {
            EventManager.OnActivateAction -= NextStepByLabelTriggerListener;
            this.manager.NextStep();
        }

        protected override void SecuredCloseStep()
        {
            this.manager.MobileTutorial.Hide();
            EventManager.OnActivateAction -= NextStepByLabelTriggerListener;
        }
    }
}

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
            titem.Id = "tutorial_activity";
            titem.InteractableObject = tutorialActivityCard.gameObject;

            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "tutorial_activity", message = "Click the first activity.", position = TutorialModel.MessagePosition.Bottom });
            this.manager.MobileTutorial.Show(queue);

            tutorialActivityCard.BtnMain.onClick.AddListener(this.DefaultExitEventListener);
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

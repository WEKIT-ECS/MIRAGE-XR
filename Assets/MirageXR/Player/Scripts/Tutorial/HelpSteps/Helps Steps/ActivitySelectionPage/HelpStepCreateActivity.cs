using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepCreateActivity : HelpStep
    {
        protected override void Init()
        {
            this._instructionText = "Tap the plus button below to add a new activity.";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            GameObject Activitytoggle = GameObject.Find("Create");

            Button button = Activitytoggle.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(SecuredExitStep);
            }
            this.highlightedObject = Activitytoggle;
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

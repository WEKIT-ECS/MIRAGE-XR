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
            this.instructionText = "Tap the plus button below to add a new activity.";
            this.highlightedObject = GameObject.Find("NewActivityToggle");//RootView_v2.Instance._searchPrefab.gameObject;
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;
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

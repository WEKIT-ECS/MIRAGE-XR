using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepChangeActivityTitleAndDescription : HelpStep
    {
        protected override void Init()
        {
            this.instructionText = "Tap on the Info tab and add a title and description";
            this.highlightedObject = GameObject.Find("Info");//RootView_v2.Instance._searchPrefab.gameObject;
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

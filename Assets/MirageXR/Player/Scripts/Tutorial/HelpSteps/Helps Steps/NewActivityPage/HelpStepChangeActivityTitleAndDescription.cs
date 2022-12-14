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
            this._instructionText = "Tap on the Info tab and add a title and description";
            GameObject info = GameObject.Find("Second");
            this.highlightedObject = info;

            Toggle toggle = info.GetComponent<Toggle>();

            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(ToggleExit);
            }

            this._shouldMove = true;

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

        private void ToggleExit(bool toggle)
        {
            SecuredExitStep();
        }
    }
}

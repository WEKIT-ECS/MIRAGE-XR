using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepRenameStep : HelpStep
    {
        protected override void Init()
        {
            this.instructionText = "Tap on edit button below the step and navigate to the Info tab where you can change the title section";
            GameObject Edit = GameObject.Find("EditButton");

            Button button = Edit.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(SecuredExitStep);
            }

            this.highlightedObject = Edit;
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

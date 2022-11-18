using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepMultipleSteps : HelpStep
    {
        protected override void Init()
        {
            this.instructionText = "Tap the + button to add new steps";

            GameObject add = GameObject.Find("AddButton");

            Button button = add.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(SecuredExitStep);
            }

            this.highlightedObject = add;
            this.shouldMove = true;
            this.pos = new Vector3(0, 0, 0);
           

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

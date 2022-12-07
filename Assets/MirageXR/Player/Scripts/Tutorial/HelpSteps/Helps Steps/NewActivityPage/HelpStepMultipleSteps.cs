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
            this._instructionText = "Tap the + button to add new steps";

            GameObject add = GameObject.Find("AddButton");

            Transform content = add.transform.parent.transform.parent;
            content.localPosition = new Vector3(content.localPosition.x, 0, content.localPosition.z);

            Button button = add.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(SecuredExitStep);
            }

            this.highlightedObject = add;
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
    }
}

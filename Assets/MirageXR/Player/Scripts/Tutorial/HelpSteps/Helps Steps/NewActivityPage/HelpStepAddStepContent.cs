using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepAddStepContent : HelpStep
    {
        protected override void Init()
        {
            this.instructionText = "To add augmentations to a step click the edit button below any action step";
            GameObject Edit = GameObject.Find("EditButton");

            Button button = Edit.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(exit);
            }

            this.highlightedObject = Edit;
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void exit()
        {
            //this.ExitStep();
            TutorialManager.Instance.showHelp(6);
        }

        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
            TutorialManager.Instance.showHelp(6);
        }




    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActionInfo : HelpSelection
    {
        protected override void Init()
        {
            this.instructionText = "";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this.popup.createNewSelectionButton("How to add step title and description").onClick.AddListener(changeTitleAndDescription);
            this.popup.createNewSelectionButton("What is an augmentation").onClick.AddListener(whatIsAnAugmentation);
            this.popup.createNewSelectionButton("How to add or change augmentations in a step").onClick.AddListener(howToAddOrChange);
            this.popup.createNewSelectionButton("Is there a way for an augmentation to stay for more than one step").onClick.AddListener(howToKeepAlive);
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void changeTitleAndDescription()
        {
            this.ExitStep();
            TutorialManager.Instance.showHelp(0);
        }

        public void whatIsAnAugmentation()
        {
            this.ExitStep();
            TutorialManager.Instance.showHelp(1);
        }

        public void howToAddOrChange()
        {
            this.ExitStep();
            TutorialManager.Instance.showHelp(2);
        }

        public void howToKeepAlive()
        {
            this.ExitStep();
            TutorialManager.Instance.showHelp(3);
        }


        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

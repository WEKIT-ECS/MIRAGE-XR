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
            this._instructionText = "";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this._popup.CreateNewSelectionButton("How to add step title and description").onClick.AddListener(changeTitleAndDescription);
            this._popup.CreateNewSelectionButton("What is an augmentation").onClick.AddListener(whatIsAnAugmentation);
            this._popup.CreateNewSelectionButton("How to add or change augmentations in a step").onClick.AddListener(howToAddOrChange);
            this._popup.CreateNewSelectionButton("Is there a way for an augmentation to stay for more than one step").onClick.AddListener(howToKeepAlive);
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void changeTitleAndDescription()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(0);
        }

        public void whatIsAnAugmentation()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(1);
        }

        public void howToAddOrChange()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(2);
        }

        public void howToKeepAlive()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(3);
        }


        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

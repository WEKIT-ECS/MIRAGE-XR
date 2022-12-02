using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionNewActivity : HelpSelection
    {
        protected override void Init()
        {
            this._instructionText = "";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this._popup.CreateNewSelectionButton("How to change activity title and description").onClick.AddListener(ChangeTitleAndDescription);
            this._popup.CreateNewSelectionButton("What is an action step").onClick.AddListener(WhatIsAnActionStep);
            this._popup.CreateNewSelectionButton("How can i make multiple steps").onClick.AddListener(SelectMultipleSteps);
            this._popup.CreateNewSelectionButton("How to rename a step").onClick.AddListener(RenameSteps);
            this._popup.CreateNewSelectionButton("How to add content to a step").onClick.AddListener(AddContent);
            this._popup.CreateNewSelectionButton("How to copy a step").onClick.AddListener(CopyStep);
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void ChangeTitleAndDescription()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(0);
        }

        public void WhatIsAnActionStep()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(1);
        }

        public void SelectMultipleSteps()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(2);
        }

        public void RenameSteps()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(3);
        }

        public void AddContent()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(4);
        }

        public void CopyStep()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(5);
        }


        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

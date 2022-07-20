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
            this.instructionText = "This is a test";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this.popup.createNewSelectionButton("How to change activity title and description").onClick.AddListener(changeTitleAndDescription);
            this.popup.createNewSelectionButton("What is an action step").onClick.AddListener(whatIsAnActionStep);
            this.popup.createNewSelectionButton("How can i make multiple steps").onClick.AddListener(selectMultipleSteps);
            this.popup.createNewSelectionButton("How to rename a step").onClick.AddListener(renameSteps);
            this.popup.createNewSelectionButton("How to add content to a step").onClick.AddListener(addContent);
            this.popup.createNewSelectionButton("How to copy a step").onClick.AddListener(copyStep);


        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void changeTitleAndDescription() {

            this.ExitStep();
            TutorialManager.Instance.showHelp(0);
        
        }

        public void whatIsAnActionStep()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(1);

        }

        public void selectMultipleSteps()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(2);

        }

        public void renameSteps()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(3);

        }
        public void addContent()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(4);

        }
        public void copyStep()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(5);

        }


        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

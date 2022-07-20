using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionTest1 : HelpSelection
    {
        protected override void Init()
        {
            this.instructionText = "This is a test";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this.popup.createNewSelectionButton("How to change title").onClick.AddListener(title);

        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void title() {

            Debug.Log("How to search");

            this.ExitStep();
            TutorialManager.Instance.showHelp(0);
        
        }



        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

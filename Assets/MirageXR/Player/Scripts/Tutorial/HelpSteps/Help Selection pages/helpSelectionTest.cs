using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionTest : HelpSelection
    {
        protected override void Init()
        {
            this.instructionText = "This is a test";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this.popup.createNewSelectionButton("How to search").onClick.AddListener(search);
            this.popup.createNewSelectionButton("How to Open").onClick.AddListener(open);
            this.popup.createNewSelectionButton("How to Edit").onClick.AddListener(edit);


        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void search() {

            Debug.Log("How to search");

            this.ExitStep();
            TutorialManager.Instance.showHelp(0);
        
        }

        public void open()
        {

            Debug.Log("How to open");

            this.ExitStep();
            TutorialManager.Instance.showHelp(1);

        }

        public void edit()
        {

            Debug.Log("How to edit");

            this.ExitStep();
            TutorialManager.Instance.showHelp(2);

        }


        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

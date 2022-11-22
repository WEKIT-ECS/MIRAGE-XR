using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActivitySelection : HelpSelection
    {
        protected override void Init()
        {
            Debug.Log("TEST2");
            this.instructionText = "This is a test";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this.popup.createNewSelectionButton("How to search for a specific activity").onClick.AddListener(search);
            this.popup.createNewSelectionButton("How to open an activity").onClick.AddListener(open);
            this.popup.createNewSelectionButton("How to create a new activity").onClick.AddListener(edit);
            this.popup.createNewSelectionButton("How to create an account and login").onClick.AddListener(createAccount);
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void search() {

            this.ExitStep();
            TutorialManager.Instance.showHelp(0);
        
        }

        public void open()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(1);

        }

        public void edit()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(2);

        }
        public void createAccount()
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

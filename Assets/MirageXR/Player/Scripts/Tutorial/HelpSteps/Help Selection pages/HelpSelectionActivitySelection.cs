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
            this._instructionText = "";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this._popup.CreateNewSelectionButton("How to search for a specific activity").onClick.AddListener(search);
            this._popup.CreateNewSelectionButton("How to open an activity").onClick.AddListener(open);
            this._popup.CreateNewSelectionButton("How to create a new activity").onClick.AddListener(edit);
            this._popup.CreateNewSelectionButton("How to create an account and login").onClick.AddListener(createAccount);
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void search() {

            this.ExitStep();
            TutorialManager.Instance.ShowHelp(0);
        }

        public void open()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(1);
        }

        public void edit()
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(2);
        }
        public void createAccount()
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

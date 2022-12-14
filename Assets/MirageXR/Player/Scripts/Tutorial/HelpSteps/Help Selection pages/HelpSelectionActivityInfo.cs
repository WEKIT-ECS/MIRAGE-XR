using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActivityInfo : HelpSelection
    {
        protected override void Init()
        {
            this._instructionText = "";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this._popup.CreateNewSelectionButton("What are title/description for").onClick.AddListener(titleAndDescription);
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void titleAndDescription() 
        {
            this.ExitStep();
            TutorialManager.Instance.ShowHelp(0);
        }


        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

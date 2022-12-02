using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepLoginRegister : HelpStep
    {
        protected override void Init()
        {
            this._instructionText = "To login, register or manage your account, click on the profile menu item below.";
            GameObject Profile = GameObject.Find("Profile");
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            Button button = Profile.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.AddListener(SecuredExitStep);
            }
            this.highlightedObject = Profile;
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

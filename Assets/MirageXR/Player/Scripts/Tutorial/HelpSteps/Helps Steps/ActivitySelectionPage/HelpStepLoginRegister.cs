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
            this.instructionText = "To login, register or manage your account, click on the profile menu item below.";
            this.highlightedObject = GameObject.Find("Profile");//RootView_v2.Instance._searchPrefab.gameObject;
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;
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

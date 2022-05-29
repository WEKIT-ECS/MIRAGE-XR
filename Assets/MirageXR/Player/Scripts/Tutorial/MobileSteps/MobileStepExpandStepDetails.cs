using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class MobileStepExpandStepDetails : MobileStep
    {
        protected override void Init()
        {
            this.instructionText = "To define what goes into this action step, click here to add more details.";
            this.highlightedObject = RootView.Instance.contentListView.BtnShowHide.gameObject;

            EventManager.MobileStepContentExpanded += DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.MobileStepContentExpanded -= DefaultExitEventListener;
        }
    }
}

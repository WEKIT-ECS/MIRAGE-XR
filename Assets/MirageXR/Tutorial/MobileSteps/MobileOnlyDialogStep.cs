using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class MobileOnlyDialogStep : MobileStep
    {
        public MobileOnlyDialogStep(string text)
        {
            this.instructionText = text;
        }

        protected override void Init()
        {
            EventManager.TutorialPopupCloseClicked += DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.TutorialPopupCloseClicked -= DefaultExitEventListener;
        }
    }
}

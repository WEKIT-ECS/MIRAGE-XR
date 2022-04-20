using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class WaitForMobilePageChangeStep : TutorialStep
    {
        protected override void SecuredEnterStep()
        {
            EventManager.MobilePageChanged += this.DefaultExitEventListener;
        }

        protected override void SecuredCloseStep()
        {
            EventManager.MobilePageChanged -= this.DefaultExitEventListener;
        }

        protected override void SecuredExitStep()
        {
            EventManager.MobilePageChanged -= this.DefaultExitEventListener;
            manager.NextStep();
        }
    }
}

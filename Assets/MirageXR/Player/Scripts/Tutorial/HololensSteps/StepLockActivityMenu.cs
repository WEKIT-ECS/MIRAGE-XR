using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class StepLockActivityMenu : ArrowHighlightingTutorialStep
    {

        protected override void Init()
        {
            this.instructionText = "When you're done positioning the Activity Menu, press this button again to lock it in place.";

            ActivitySelectionMenu activitySelectionMenu = Object.FindObjectOfType<ActivitySelectionMenu>();
            this.highlightedObject = activitySelectionMenu.LockButton;
            EventManager.ActivitySelectionMenuLockClicked += this.LockClickListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        private void LockClickListener()
        {
            try
            {
                SpriteToggle toggle = this.highlightedObject.GetComponent<SpriteToggle>();
                if (!toggle.IsSelected)
                {
                    ExitStep();
                }
            }
            catch
            {
                Debug.LogError("SpriteToggle component missing in Lock for StepLockActivityMenu in Tutorial.");
                ExitStep();
            }

        }

        protected override void Detach()
        {
            EventManager.ActivitySelectionMenuLockClicked -= this.LockClickListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

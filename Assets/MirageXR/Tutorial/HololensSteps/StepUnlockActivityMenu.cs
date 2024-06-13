using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to unlock the ActivityMenu for movement.
    /// </summary>
    public class StepUnlockActivityMenu : ArrowHighlightingTutorialStep
    {

        protected override void Init()
        {
            this.instructionText = "Hello! Welcome to the tutorial of MirageXR. Let's start with the basics of organizing your 3D workspace. Windows are locked in 3D positions at the start. Use the \"Tap\" gesture on this lock icon to unlock this window.";

            ActivitySelectionMenu activitySelectionMenu = Object.FindObjectOfType<ActivitySelectionMenu>();
            this.highlightedObject = activitySelectionMenu.LockButton;
            EventManager.ActivitySelectionMenuLockClicked += this.UnlockClickListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        private void UnlockClickListener()
        {
            try
            {
                SpriteToggle toggle = this.highlightedObject.GetComponent<SpriteToggle>();
                if (toggle.IsSelected)
                {
                    ExitStep();
                }
            }
            catch
            {
                Debug.LogError("SpriteToggle component missing in Lock for StepUnlockActivityMenu in TutorialUI.");
                ExitStep();
            }

        }

        protected override void Detach()
        {
            EventManager.ActivitySelectionMenuLockClicked -= this.UnlockClickListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

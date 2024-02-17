using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to move the ActivityMenu.
    /// </summary>
    public class StepDragActivityMenu : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "Once unlocked, you can use the \"Pinch and hold\" gesture on this menu panel to move it. Try to move the window to go to the next step.";

            ActivitySelectionMenu activitySelectionMenu = Object.FindObjectOfType<ActivitySelectionMenu>();
            this.highlightedObject = activitySelectionMenu.Header;
            EventManager.ActivitySelectionMenuDragEnd += this.DefaultExitEventListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            EventManager.ActivitySelectionMenuDragEnd -= this.DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to create a new activity by
    /// pressing the + button on the ActivitySelectionMenu.
    /// </summary>
    public class StepCreateNewActivity : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "This window is an \"Activity Menu\", which contains a list of learning activities that are available on your Moodle. You can also create new learning activities using this menu. Let's create a new activity by \"Tap\" this + button.";

            ActivitySelectionMenu activitySelectionMenu = Object.FindObjectOfType<ActivitySelectionMenu>();
            this.highlightedObject = activitySelectionMenu.ActivityCreationButton;
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }
    }
}

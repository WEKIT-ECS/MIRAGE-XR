using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to rename the current activity.
    /// </summary>
    public class StepRenameActivity : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "It is always a good idea to give a proper name to your learning activity. You can \"Tap\" this field to rename your learning activity.";

            ActionListMenu actionListMenu = Object.FindObjectOfType<ActionListMenu>();
            this.highlightedObject = actionListMenu.GetTitleText().gameObject;
            EventManager.ActivityRenamed += DefaultExitEventListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            EventManager.ActivityRenamed -= DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

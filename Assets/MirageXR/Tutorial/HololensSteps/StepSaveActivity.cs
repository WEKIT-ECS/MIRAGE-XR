using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to save the current activity.
    /// </summary>
    public class StepSaveActivity : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "During your work, make sure you save your progress regularly. \"Tap\" this button to save your progress";

            ActivityEditor editor = ActivityEditor.Instance;
            this.highlightedObject = editor.SaveButton.gameObject;

            EventManager.ActivitySaveButtonClicked += this.DefaultExitEventListener;
            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            EventManager.ActivitySaveButtonClicked -= this.DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

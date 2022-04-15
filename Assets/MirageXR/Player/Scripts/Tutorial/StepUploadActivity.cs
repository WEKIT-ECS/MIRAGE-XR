using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to upload the current activity.
    /// </summary>
    public class StepUploadActivity : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "Once you finished creating your learning activity, \"Tap\" this button to upload your work to the Cloud.";

            ActivityEditor editor = ActivityEditor.Instance;
            this.highlightedObject = editor.GetUploadButton().gameObject;

            EventManager.ActivityUploadButtonClicked += this.DefaultExitEventListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            EventManager.ActivityUploadButtonClicked -= this.DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

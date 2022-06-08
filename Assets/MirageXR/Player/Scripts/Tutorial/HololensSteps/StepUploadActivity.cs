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
            this.instructionText = "Well done, you are almost done with the tutorial! " +
                "The only thing that remains to be done is to upload your activity to the cloud. " +
                "To do this, Tap this button.";

            ActivityEditor editor = ActivityEditor.Instance;
            this.highlightedObject = editor.UploadButton.gameObject;

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

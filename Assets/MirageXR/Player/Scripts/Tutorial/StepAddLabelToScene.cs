using System.Collections;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user presses the "Accept" button, adding the label to the scene.
    /// </summary>
    public class StepAddLabelToScene : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "You can add a Label to your environment by \"Tap\" this button.";
            this.arrowRotationOffset = Vector3.up * -30f;

            LabelEditor editor = Object.FindObjectOfType<LabelEditor>();
            this.highlightedObject = editor.GetAcceptButton();
            EventManager.OnToggleObject += LabelCreationListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        private void LabelCreationListener(ToggleObject label, bool activated)
        {
            manager.CreatedLabel = label;
            ExitStep();
        }

        protected override void Detach()
        {

            EventManager.OnToggleObject -= LabelCreationListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

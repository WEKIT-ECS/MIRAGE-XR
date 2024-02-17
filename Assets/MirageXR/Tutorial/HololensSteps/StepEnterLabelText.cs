using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to give the new label its text.
    /// </summary>
    public class StepEnterLabelText : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "A label can provide a short description for your students, let's add a short description by \"Tap\" and edit this text box.";
            this.arrowRotationOffset = Vector3.up * -30f;
            this.arrowPositionOffset = Vector3.up * 0.02f;

            LabelEditor editor = Object.FindObjectOfType<LabelEditor>();
            this.highlightedObject = editor.gameObject;
            EventManager.LabelEditorTextChanged += this.DefaultExitEventListener;
            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            EventManager.LabelEditorTextChanged -= this.DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

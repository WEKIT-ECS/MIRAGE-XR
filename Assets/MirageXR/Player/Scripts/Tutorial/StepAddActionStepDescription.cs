using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{ 
    /// <summary>
    /// Scenario: The user is asked to enter a description for an action step.
    /// </summary>
    public class StepAddActionStepDescription : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "You can also add a short description of the step. \"Tap\" here to start editing a step description.";

            TaskStationDetailMenu menu = TaskStationDetailMenu.Instance;
            this.highlightedObject = menu.ActionDescriptionInputField.gameObject;

            EventManager.ActionStepDescriptionInputChanged += this.DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.ActionStepDescriptionInputChanged -= this.DefaultExitEventListener;
        }
    }
}

using UnityEngine;


namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to create a new action step by
    /// pressing the + button on the ActionListMenu.
    /// </summary>
    public class StepCreateNewActionStep : ArrowHighlightingTutorialStep
    {
        private void ActionCreatedListener(Action action)
        {
            ExitStep();
        }

        protected override void Init()
        {
            this.instructionText = "Your learning activity should consist of multiple steps called “Actions” to progressively guide your students. To create a new step, \"Tap\" this + button.";

            ActionListMenu actionListMenu = Object.FindObjectOfType<ActionListMenu>();
            this.highlightedObject = actionListMenu.AddActionStepButton;
            EventManager.OnActionCreated += ActionCreatedListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            EventManager.OnActionCreated -= ActionCreatedListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

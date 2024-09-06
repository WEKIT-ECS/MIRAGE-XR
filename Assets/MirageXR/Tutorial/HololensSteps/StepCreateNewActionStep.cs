using UnityEngine;


namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to create a new action step by
    /// pressing the + button on the ActionListMenu.
    /// </summary>
    public class StepCreateNewActionStep : ArrowHighlightingTutorialStep
    {
        private void ActionCreatedListener(LearningExperienceEngine.Action action)
        {
            ExitStep();
        }

        protected override void Init()
        {
            this.instructionText = "Your learning activity should consist of multiple steps called ?Actions? to progressively guide your students. To create a new step, \"Tap\" this + button.";

            ActionListMenu actionListMenu = Object.FindObjectOfType<ActionListMenu>();
            this.highlightedObject = actionListMenu.AddActionStepButton;
            LearningExperienceEngine.EventManager.OnActionCreated += ActionCreatedListener;

            LearningExperienceEngine.EventManager.OnStartActivity += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            LearningExperienceEngine.EventManager.OnActionCreated -= ActionCreatedListener;
            LearningExperienceEngine.EventManager.OnStartActivity -= DefaultCloseEventListener;
        }
    }
}

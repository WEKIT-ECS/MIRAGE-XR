using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to add a title to an action step.
    /// </summary>
    public class StepAddActionStepTitle : ArrowHighlightingTutorialStep
    {
        private InputField titleField;

        protected override void Init()
        {
            this.instructionText = "After creating a new Action, it is always good practice to provide a title for the Action step for your students, \"Tap\" this text box to give the step a title.";

            TaskStationDetailMenu menu = TaskStationDetailMenu.Instance;
            this.titleField = menu.ActionTitleInputField;
            this.highlightedObject = titleField.gameObject;

            EventManager.ActionStepTitleChanged += TitleChangedListener;
            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        private void TitleChangedListener()
        {
            if (titleField.text != "" && !titleField.text.Equals("Action Step 2"))
            {
                this.ExitStep();
            }
        }

        protected override void Detach()
        {
            EventManager.ActionStepTitleChanged -= this.DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

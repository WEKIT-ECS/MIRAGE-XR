using UnityEngine;


namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to delete an action step (in this case
    /// the first step in the list) on the ActionListMenu.
    /// </summary>
    public class StepDeleteActionStep : ArrowHighlightingTutorialStep
    {

        protected override void Init()
        {
            this.instructionText = "If you are unsatisfied with a learning step, you can delete a step by \"Tap\" this button.";

            ActionListMenu actionListMenu = ActionListMenu.Instance;
            ActionListItem targetItem = actionListMenu.ActionListItems[0];
            this.highlightedObject = targetItem.GetDeleteButton().gameObject;

            EventManager.OnActionDeleted += ActionDeletedListener;
            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        private void ActionDeletedListener(string actionId)
        {
            ExitStep();
        }

        protected override void Detach()
        {
            EventManager.OnActionDeleted -= ActionDeletedListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

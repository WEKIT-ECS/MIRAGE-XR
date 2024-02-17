using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to create a new augmentation by
    /// pressing the + button on the Taskstation.
    /// </summary>
    public class StepCreateNewAugmentation : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "In each Action, you can design an environment to guide your students. The learning environment will consist of augmented virtual objects called Augmentations. \"Tap\" on this + button to bring up a list of them.";

            TaskStationDetailMenu taskStationDetailMenu = Object.FindObjectOfType<TaskStationDetailMenu>();
            this.highlightedObject = taskStationDetailMenu.AddAugmentationButton;
            EventManager.AddAugmentationButtonClicked += DefaultExitEventListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            EventManager.AddAugmentationButtonClicked -= DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

using LearningExperienceEngine;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to rename the current activity.
    /// </summary>
    public class StepRenameActivity : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "It is always a good idea to give a proper name to your learning activity. You can \"Tap\" this field to rename your learning activity.";

            ActionListMenu actionListMenu = Object.FindObjectOfType<ActionListMenu>();
            this.highlightedObject = actionListMenu.TitleText.gameObject;
            LearningExperienceEngine.EventManager.OnActivityRenamed += DefaultExitEventListener;

            LearningExperienceEngine.EventManager.OnStartActivity += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            LearningExperienceEngine.EventManager.OnActivityRenamed -= DefaultExitEventListener;
            LearningExperienceEngine.EventManager.OnStartActivity -= DefaultCloseEventListener;
        }
    }
}

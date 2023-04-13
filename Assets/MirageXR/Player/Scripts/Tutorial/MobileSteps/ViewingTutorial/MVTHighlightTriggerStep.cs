using UnityEngine;

namespace MirageXR
{
    public class MVTHighlightTriggerStep : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "This list contains different types of Annotation. Let's start by creating a basic Label annotation, \"Tap\" to select Label from this list.";

            GameObject target = GameObject.Find("AN-867c133b-2d2d-4f89-b170-b2838766ff09");
            target = target.transform.FindDeepChild("act:Highlight").gameObject;

            highlightedObject = target;

            //this.arrowPositionOffset = Vector3.forward * (-0.001f) + Vector3.up * 0.02f;

            EventManager.OnActivateAction += NextStepByHighlightTriggerListener;
        }

        private void NextStepByHighlightTriggerListener(string action)
        {
            ExitStep();
        }

        protected override void Detach()
        {
            EventManager.OnActivateAction -= NextStepByHighlightTriggerListener;
        }
    }
}

using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class MVTLabelTriggerStep : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "This list contains different types of Annotation. Let's start by creating a basic Label annotation, \"Tap\" to select Label from this list.";

            GameObject target = GameObject.Find("AN-076be810-8ec7-4f1f-8752-21fef7ff4954");
            target = target.transform.FindDeepChild("TextObject").gameObject;

            highlightedObject = target;

            //this.arrowPositionOffset = Vector3.forward * (-0.001f) + Vector3.up * 0.02f;

            EventManager.OnActivateAction += ExitListener;
        }

        private async void ExitListener(string action)
        {
            await Task.Delay(100);
            ExitStep();
        }

        protected override void Detach()
        {
            EventManager.OnActivateAction -= ExitListener;
        }
    }
}

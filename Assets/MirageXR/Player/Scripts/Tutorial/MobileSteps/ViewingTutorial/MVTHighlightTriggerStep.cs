using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class MVTHighlightTriggerStep : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "This augmentation is called a Highlight or Focus. It can be used to help you find things in AR. It can also advance steps in an activity with a trigger, just the same as the Label before. To go to the next step, look at its centre.";

            GameObject target = GameObject.Find("AN-867c133b-2d2d-4f89-b170-b2838766ff09");
            target = target.transform.FindDeepChild("act:Highlight").gameObject;

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

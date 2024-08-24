using LearningExperienceEngine;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class MVTLabelTriggerStep : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "This here is a Label, a type of augmentation, the most basic one really. It has a special feature attached to it called a trigger, which in this case advances us to the next step. To do so, simply point the camera at the center.";

            GameObject target = GameObject.Find("AN-076be810-8ec7-4f1f-8752-21fef7ff4954");
            target = target.transform.FindDeepChild("TextObject").gameObject;

            highlightedObject = target;

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

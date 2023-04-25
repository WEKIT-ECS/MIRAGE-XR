using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class MVTGhostTrackStep : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "This is a far more advanced augmentation called a Ghost Track. It's a recording of another user's movement and voice. To advance to the next step, use the UI and click 'Next Step'.";

            GameObject target = GameObject.Find("AN-c37d0117-a7d0-4de1-94d6-290a3369b23a");
            target = target.transform.FindDeepChild("Head").gameObject;

            highlightedObject = target;

            this.arrowRotationOffset = new Vector3(0f, 180f, 0f);

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

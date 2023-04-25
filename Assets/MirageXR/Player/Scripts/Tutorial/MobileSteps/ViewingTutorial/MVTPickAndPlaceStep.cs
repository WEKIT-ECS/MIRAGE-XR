using UnityEngine;

namespace MirageXR
{
    public class MVTPickAndPlaceStep : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "The arrow you see here is part of the Pick&Place augmentation. To interact with it tap and hold the arrow and move it around. To complete the tutorial, place the arrow on its target, the robot!";

            GameObject target = GameObject.Find("AN-c29b5f37-3ba2-4256-bc4d-45463e2a32ea");
            target = target.transform.FindDeepChild("Canvas").gameObject;

            highlightedObject = target;

            //this.arrowPositionOffset = Vector3.forward * (-0.001f) + Vector3.up * 0.02f;

            EventManager.OnPickPlacedCorrectly += this.DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.OnPickPlacedCorrectly -= this.DefaultExitEventListener;
        }
    }
}

using UnityEngine;

namespace MirageXR
{
    public class TestUIStep : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "This list contains different types of Annotation. Let's start by creating a basic Label annotation, \"Tap\" to select Label from this list.";

            GameObject target = GameObject.Find("AN-c37d0117-a7d0-4de1-94d6-290a3369b23a");
            target = target.transform.FindDeepChild("Head").gameObject;

            highlightedObject = target;

            //this.arrowPositionOffset = Vector3.forward * (-0.001f) + Vector3.up * 0.02f;

            //highlightedObject.GetComponent<PoiAddItem>().OnPoiAddItemClicked += LabelItemClickedListener;

            //EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        private void LabelItemClickedListener(ContentType type)
        {
            ExitStep();
        }

        protected override void Detach()
        {
            //highlightedObject.GetComponent<PoiAddItem>().OnPoiAddItemClicked -= LabelItemClickedListener;
            //EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }


    }
}

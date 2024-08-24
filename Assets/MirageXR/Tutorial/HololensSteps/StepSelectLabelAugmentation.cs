using LearningExperienceEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to select the label augmentation
    /// from the list of available augmentation types.
    /// </summary>
    public class StepSelectLabelAugmentation : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "This list contains different types of Annotation. Let's start by creating a basic Label annotation, \"Tap\" to select Label from this list.";

            // TODO: This should definitely be improved at some point.
            TaskStationDetailMenu menu = Object.FindObjectOfType<TaskStationDetailMenu>();
            Transform tmp = menu.transform.FindDeepChild("AnnotationAddMenu");
            highlightedObject = tmp.GetChild(4).gameObject;

            this.arrowPositionOffset = Vector3.forward * (-0.001f) + Vector3.up * 0.02f;

            highlightedObject.GetComponent<PoiAddItem>().OnPoiAddItemClicked += LabelItemClickedListener;

            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        private void LabelItemClickedListener(LearningExperienceEngine.ContentType type)
        {
            ExitStep();
        }

        protected override void Detach()
        {
            highlightedObject.GetComponent<PoiAddItem>().OnPoiAddItemClicked -= LabelItemClickedListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }

       
    }
}

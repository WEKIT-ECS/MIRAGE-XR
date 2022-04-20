using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to move the action editor (TaskStationEditor).
    /// </summary>
    public class StepDragActionEditor : ArrowHighlightingTutorialStep
    {

        /// <summary>
        /// The task station is not loaded instantly. That is why it is necessary
        /// to listen to the event in order to make sure the TaskStationEditor is
        /// enabled before continuing.
        /// </summary>
        protected override void SecuredEnterStep()
        {
            EventManager.TaskStationEditorEnabled += TaskStationEditorEnabledListener;
        }

        private void TaskStationEditorEnabledListener()
        {
            EventManager.TaskStationEditorEnabled -= TaskStationEditorEnabledListener;
            Init();
            SetupArrow();            
        }

        protected override void Init()
        {
            this.instructionText = "Look below (slowly)! You will see an orange circle around you, this is your Workstation. Every object in your lesson will be positioned based on the workstation. To move the workstation, you can use the \"Pinch and hold\" gesture.";
            this.arrowRotationOffset = Vector3.up * -30f;

            TaskStationEditor taskStationEditor = Object.FindObjectOfType<TaskStationEditor>();
            this.highlightedObject = taskStationEditor.gameObject;
            EventManager.TaskStationEditorDragEnd += DefaultExitEventListener;
            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }

        protected override void Detach()
        {
            EventManager.TaskStationEditorDragEnd -= DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}
using UnityEngine;
using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.EventSystems;

namespace MirageXR
{
    /// <summary>
    /// Scenario: The user is asked to move the created label.
    /// </summary>
    public class StepMoveCreatedLabel : ArrowHighlightingTutorialStep
    {
        protected override void Init()
        {
            this.instructionText = "Your newly created Label will be created at this red sphere called Spawn Point. Every annotation will spawn at this location, so let move this Label out of the way by using the \"Pinch and hold\" gesture.";
            this.arrowRotationOffset = Vector3.up * -10f;

            GameObject label = GameObject.Find(manager.CreatedLabel.id + "/" + manager.CreatedLabel.poi);
            this.highlightedObject = label;
            EventManager.AugmentationPoiChanged += this.DefaultExitEventListener;
            EventManager.OnActivityStarted += DefaultCloseEventListener;
        }
        //
        protected override void Detach()
        {
            EventManager.AugmentationPoiChanged -= this.DefaultExitEventListener;
            EventManager.OnActivityStarted -= DefaultCloseEventListener;
        }
    }
}

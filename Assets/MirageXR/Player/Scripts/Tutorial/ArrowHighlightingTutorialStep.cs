using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// This class serves as a template for all tutorial steps that use the
    /// TutorialArrow as the highlighting feature.
    /// </summary>
    public abstract class ArrowHighlightingTutorialStep : TutorialStep
    {
        protected string instructionText;
        protected GameObject arrow;

        protected Vector3? arrowPositionOffset;
        protected Vector3? arrowRotationOffset;

        protected override void SecuredEnterStep()
        {
            Init();
            SetupArrow();
        }

        protected override void SecuredExitStep()
        {
            RemoveArrow();
            Detach();
            manager.NextStep();
        }

        protected override void SecuredCloseStep()
        {
            RemoveArrow();
            Detach();
        }

        /// <summary>
        /// This method serves to setup the fields of the TutorialStep
        /// as well as connect listeners to the relevant completion event.
        /// It should also define Arrow offsets if needed.
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// This method should create the needed highlighting arrow GameObject
        /// and set its focus object and instruction text.
        /// </summary>
        protected void SetupArrow()
        {
            if (highlightedObject != null)
            {
                TutorialArrowFactory factory = TutorialArrowFactory.Instance();
                this.arrow = factory.CreateArrow();
                TutorialArrow arrowScript = arrow.GetComponent<TutorialArrow>();

                if (arrowScript is Tutorial3DArrow)
                {
                    Tutorial3DArrow arrowScript3D = (Tutorial3DArrow)arrowScript;
                    arrowScript3D.PositionOffset = arrowPositionOffset;
                    arrowScript3D.RotationOffset = arrowRotationOffset;
                }

                arrowScript.PointTo(highlightedObject, instructionText);
            }
            else
            {
                Debug.LogError("Highlighted object missing in tutorial step: " + this.GetType().Name);
                manager.CloseTutorial();
            }
        }

        /// <summary>
        /// Remove listeners from events that were enabled in Init().
        /// </summary>
        protected abstract void Detach();

        /// <summary>
        /// Removes the arrow from the scene and destroys it.
        /// </summary>
        protected void RemoveArrow()
        {
            if (arrow != null)
            {
                TutorialArrow arrowScript = arrow.GetComponent<TutorialArrow>();
                arrowScript.Dissapear();
                Object.Destroy(arrow);
            }

        }
        
        /// <summary>
        /// This function simply calles the ExitStep() of its step.
        /// It is defined here as a separate action so it can be used
        /// in concrete Steps to attach events to.
        /// </summary>
        protected void DefaultExitEventListener()
        {
            ExitStep();
        }


        /// <summary>
        /// If the user starts an activity or creates a new one when unexpected,
        /// it will break the tutorial. This method closes the tutorial to prevent
        /// adverce effects happening.
        /// </summary>
        protected void DefaultCloseEventListener()
        {
            manager.CloseTutorial();
        }

    }
}

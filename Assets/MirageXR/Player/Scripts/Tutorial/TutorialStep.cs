using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Abstract class defining the required fields and methods of all steps.
    /// </summary>
    public abstract class TutorialStep
    {
        /// <summary>
        /// The GameObject that the step will be working with,
        /// that is highlighting, adding text description etc.
        /// </summary>
        public GameObject highlightedObject;

        /// <summary>
        /// Reference to the TutorialManager handling the step.
        /// </summary>
        public TutorialManager manager;

        /// <summary>
        /// Status field for if the step regards itself as active or not.
        /// Used to make sure steps are not re-entered if they are active and
        /// that they can't be exited or closed if they are not active.
        /// </summary>
        public bool IsActive { get; protected set; }

        public TutorialStep()
        {
            this.manager = TutorialManager.Instance();
            this.IsActive = false;
        }

        /// <summary>
        /// This method is called by the TutorialManager when the
        /// step should be started. The method should set up
        /// all listeners for events, highlight GameObjects, and other
        /// actions required for starting the step.
        /// </summary>
        public void EnterStep()
        {
            if (!IsActive)
            {
                this.IsActive = true;
                this.SecuredEnterStep();
            }
            else
            {
                return;
            }
        }

        protected abstract void SecuredEnterStep();

        /// <summary>
        /// This method should detach the step from any events and generally undo
        /// any actions done by EnterStep() that should not be permanent. It then
        /// notifies the TutorialManager that the step is over.
        /// </summary>
        public void ExitStep()
        {
            if (IsActive)
            {
                this.IsActive = false;
                this.SecuredExitStep();
            }
            else
            {
                return;
            }
        }

        protected abstract void SecuredExitStep();

        /// <summary>
        /// This method is called by the TutorialManager when the tutorial is
        /// closed before being fully completed, i.e. the TutorialButton is pressed
        /// in the middle of a step. The method should remove all step features such
        /// as highlighting and remove event- and other listeners, undoing EnterStep().
        /// Unlike ExitStep() it should not inform the TutorialManager, as it is called
        /// directly by it.
        /// </summary>
        public void CloseStep()
        {
            if (IsActive)
            {
                this.SecuredCloseStep();
            }
            else
            {
                return;
            }
        }

        protected abstract void SecuredCloseStep();

    }
}

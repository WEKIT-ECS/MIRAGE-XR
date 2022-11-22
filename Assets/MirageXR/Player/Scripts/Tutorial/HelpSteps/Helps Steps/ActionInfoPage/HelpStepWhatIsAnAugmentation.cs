using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepWhatIsAnAugmentation : HelpStep
    {
        protected override void Init()
        {
            this.instructionText = "Augmentations in MirageXR are primitives that comprise the holographic part of the training experience of the user. A trainer can use different types of augmentations to construct the holographic experience, while the trainee perceives them as part of the learning experience.";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

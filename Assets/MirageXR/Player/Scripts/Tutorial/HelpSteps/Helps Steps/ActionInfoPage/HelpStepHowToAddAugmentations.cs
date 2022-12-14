using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepHowToAddAugmentations : HelpStep
    {
        protected override void Init()
        {
            this._instructionText = "Additional options for added augmentations can be accessed by pressing the three dots on the right side of an augmentation tile.";
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

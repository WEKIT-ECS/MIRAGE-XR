using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepWhatIsCalibration : HelpStep
    {
        protected override void Init()
        {
            this._instructionText = "Learning in the real world is most effective when the activities are responsive to points of interest and events in the real world. MirageXR uses a single calibration marker to tie the digital augmentations to points of interest in the physical environment of the user.";
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

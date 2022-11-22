using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepWhyCalibrate : HelpStep
    {
        protected override void Init()
        {
            this.instructionText = "Calibration is needed to tie the digital augmentations to points of interest in the physical environment. Where locations do not matter, the calibration marker can be hung somewhere where enough open space is provided to place the holograms. Where locations matter, like, for example, when instructing in a maker’s lab on how to handle a 3D printer, the activity will either include instruction on where to place the calibration marker directly or a hand -out will show students or teachers how to. ";
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

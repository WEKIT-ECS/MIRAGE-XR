using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepHowToCalibrate : HelpStep
    {
        protected override void Init()
        {
            this._instructionText = "To calibrate, you need to download the calibration marker from https://wekit-ecs.com/documents/calibration, print it on paper, and hang in your workspace. Calibration itself is run simply by gazing at the calibration marker.";
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

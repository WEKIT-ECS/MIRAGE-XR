using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepActivityInfo : HelpStep
    {
        protected override void Init()
        {
            this._instructionText = "Titles allow you to name your steps, making it clearer for creator and learner what a step is about. Descriptions allow you to go into even more detail, for example by adding instructions to the step.";
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

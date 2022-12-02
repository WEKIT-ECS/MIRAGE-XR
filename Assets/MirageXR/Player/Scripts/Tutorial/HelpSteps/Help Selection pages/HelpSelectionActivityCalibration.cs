using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActivityCalibration : HelpSelection
    {
        protected override void Init()
        {
            this._instructionText = "";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this._popup.CreateNewSelectionButton("What is calibration").onClick.AddListener(whatIsCalibration);
            this._popup.CreateNewSelectionButton("How do I calibrate").onClick.AddListener(howToCalibrate);
            this._popup.CreateNewSelectionButton("Why do I need to calibrate").onClick.AddListener(whyCalibrate);
            this._popup.CreateNewSelectionButton("What can I use as a calibration image").onClick.AddListener(whatImage);
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void whatIsCalibration() {

            this.ExitStep();
            TutorialManager.Instance.ShowHelp(0);
        
        }

        public void howToCalibrate()
        {

            this.ExitStep();
            TutorialManager.Instance.ShowHelp(1);

        }

        public void whyCalibrate()
        {

            this.ExitStep();
            TutorialManager.Instance.ShowHelp(2);

        }

        public void whatImage()
        {

            this.ExitStep();
            TutorialManager.Instance.ShowHelp(3);

        }

        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

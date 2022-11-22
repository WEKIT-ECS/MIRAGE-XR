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
            this.instructionText = "";
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            this.popup.createNewSelectionButton("What is calibration").onClick.AddListener(whatIsCalibration);
            this.popup.createNewSelectionButton("How do I calibrate").onClick.AddListener(howToCalibrate);
            this.popup.createNewSelectionButton("Why do I need to calibrate").onClick.AddListener(whyCalibrate);
            this.popup.createNewSelectionButton("What Can I use as a calibration image").onClick.AddListener(whatImage);
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        public void whatIsCalibration() {

            this.ExitStep();
            TutorialManager.Instance.showHelp(0);
        
        }

        public void howToCalibrate()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(1);

        }

        public void whyCalibrate()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(2);

        }

        public void whatImage()
        {

            this.ExitStep();
            TutorialManager.Instance.showHelp(3);

        }

        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}

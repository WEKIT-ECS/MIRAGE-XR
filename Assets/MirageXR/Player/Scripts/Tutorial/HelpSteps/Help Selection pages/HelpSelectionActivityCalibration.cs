using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActivityCalibration
    {
        private Tutorial _mobileTutorial;
        private HelpSelectionPopup _popup;

        public void Init(HelpSelectionPopup popup, Tutorial mobileTutorial)
        {
            _popup = popup;
            _mobileTutorial = mobileTutorial;

            _popup.CreateNewSelectionButton("What is calibration").onClick.AddListener(whatIsCalibration);
            _popup.CreateNewSelectionButton("How do I calibrate").onClick.AddListener(howToCalibrate);
            _popup.CreateNewSelectionButton("Why do I need to calibrate").onClick.AddListener(whyCalibrate);
            _popup.CreateNewSelectionButton("What can I use as a calibration image").onClick.AddListener(whatImage);
        }

        public void whatIsCalibration() {

            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "Learning in the real world is most effective when the activities are responsive to points of interest and events in the real world. MirageXR uses a single calibration marker to tie the digital augmentations to points of interest in the physical environment of the user." });
            _mobileTutorial.Show(queue);
        }

        public void howToCalibrate()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "To calibrate, you need to download the calibration marker from https://wekit-ecs.com/documents/calibration, print it on paper, and hang in your workspace. Calibration itself is run simply by gazing at the calibration marker." });
            _mobileTutorial.Show(queue);
        }

        public void whyCalibrate()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "Calibration is needed to tie the digital augmentations to points of interest in the physical environment. Where locations do not matter, the calibration marker can be hung somewhere where enough open space is provided to place the holograms. Where locations matter, like, for example, when instructing in a maker’s lab on how to handle a 3D printer, the activity will either include instruction on where to place the calibration marker directly or a hand -out will show students or teachers how to." });
            _mobileTutorial.Show(queue);
        }

        public void whatImage()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "The calibration marker can be downloaded from https://wekit-ecs.com/documents/calibration. More images will be available as markers soon." });
            _mobileTutorial.Show(queue);
        }
    }
}

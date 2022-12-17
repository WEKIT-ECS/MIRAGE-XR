using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActionAugmentations
    {
        private Tutorial _mobileTutorial;
        private HelpSelectionPopup _popup;

        public void Init(HelpSelectionPopup popup, Tutorial mobileTutorial)
        {
            _popup = popup;
            _mobileTutorial = mobileTutorial;

            _popup.CreateNewSelectionButton("How to add step title and description").onClick.AddListener(changeTitleAndDescription);
            _popup.CreateNewSelectionButton("What is an augmentation").onClick.AddListener(whatIsAnAugmentation);
            _popup.CreateNewSelectionButton("How to add augmentations to a step").onClick.AddListener(howToAdd);
            _popup.CreateNewSelectionButton("How to make changes to existing augmentations").onClick.AddListener(howToChange);
            _popup.CreateNewSelectionButton("Is there a way for an augmentation to stay for more than one step").onClick.AddListener(howToKeepAlive);
        }

        public void changeTitleAndDescription()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "step_info", message = "You can add or change the title and description of any step, just switch to the info tab." });
            _mobileTutorial.Show(queue);
        }

        public void whatIsAnAugmentation()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "Augmentations in MirageXR are primitives that comprise the holographic part of the training experience of the user. A trainer can use different types of augmentations to construct the holographic experience, while the trainee perceives them as part of the learning experience." });
            _mobileTutorial.Show(queue);
        }

        public void howToAdd()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "step_add_augmentation", message = "Additional options for added augmentations can be accessed by pressing the three dots on the right side of an augmentation tile.", position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        public void howToChange()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "Additional options for added augmentations can be accessed by pressing the three dots on the right side of an augmentation tile." });
            _mobileTutorial.Show(queue);
        }

        public void howToKeepAlive()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "You can make the same augmentation to be present in multiple steps. Tap on the settings of the augmentation, choose keep-alive, and select the range of steps." });
            _mobileTutorial.Show(queue);
        }
    }
}

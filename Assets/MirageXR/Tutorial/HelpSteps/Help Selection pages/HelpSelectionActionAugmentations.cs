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

        public void Init(HelpSelectionPopup popup, Tutorial mobileTutorial, bool editMode)
        {
            _popup = popup;
            _mobileTutorial = mobileTutorial;

            _popup.CreateNewSelectionButton("What is an augmentation").onClick.AddListener(WhatIsAnAugmentation);
            _popup.CreateNewSelectionButton("How to find augmentations").onClick.AddListener(HowToFindAugmentations);
            if (editMode)
            {
                _popup.CreateNewSelectionButton("How to add step title and description").onClick.AddListener(ChangeTitleAndDescription);
                _popup.CreateNewSelectionButton("How to add augmentations to a step").onClick.AddListener(HowToAdd);
                _popup.CreateNewSelectionButton("How to make changes to existing augmentations").onClick.AddListener(HowToChange);
                _popup.CreateNewSelectionButton("Is there a way for an augmentation to stay for more than one step").onClick.AddListener(HowToKeepAlive);
            }
        }

        private void WhatIsAnAugmentation()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Message = "Augmentations in MirageXR are primitives that comprise the holographic part of the training experience of the user. A trainer can use different types of augmentations to construct the holographic experience, while the trainee perceives them as part of the learning experience." });
            _mobileTutorial.Show(queue);
        }

        private void HowToFindAugmentations()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Message = "Look down to your feet and you will see an aura surrounding you, with connecting lines leading to where the action happens." });
            _mobileTutorial.Show(queue);
        }

        private void ChangeTitleAndDescription()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "step_info", Message = "You can add or change the title and description of any step, just switch to the info tab." });
            _mobileTutorial.Show(queue);
        }

        private void HowToAdd()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "step_add_augmentation", Message = "Additional options for added augmentations can be accessed by pressing the three dots on the right side of an augmentation tile.", Position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        private void HowToChange()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Message = "Additional options for added augmentations can be accessed by pressing the three dots on the right side of an augmentation tile." });
            _mobileTutorial.Show(queue);
        }

        private void HowToKeepAlive()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Message = "You can make the same augmentation to be present in multiple steps. Tap on the settings of the augmentation, choose keep-alive, and select the range of steps." });
            _mobileTutorial.Show(queue);
        }
    }
}

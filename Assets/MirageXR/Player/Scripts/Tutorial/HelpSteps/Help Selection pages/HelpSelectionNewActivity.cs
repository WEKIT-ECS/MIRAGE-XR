using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionNewActivity
    {
        private Tutorial _mobileTutorial;
        private HelpSelectionPopup _popup;

        public void Init(HelpSelectionPopup popup, Tutorial mobileTutorial, bool editMode)
        {
            _popup = popup;
            _mobileTutorial = mobileTutorial;

            _popup.CreateNewSelectionButton("What is an action step").onClick.AddListener(WhatIsAnActionStep);
            if (editMode)
            {
                _popup.CreateNewSelectionButton("How to change activity title and description").onClick.AddListener(ChangeTitleAndDescription);
                _popup.CreateNewSelectionButton("How can i make multiple steps").onClick.AddListener(SelectMultipleSteps);
                _popup.CreateNewSelectionButton("How to rename a step").onClick.AddListener(RenameSteps);
                _popup.CreateNewSelectionButton("How to add content to a step").onClick.AddListener(AddContent);
                //_popup.CreateNewSelectionButton("How to copy a step").onClick.AddListener(CopyStep); TODO: Add this when we can actually copy
            }
        }

        public void ChangeTitleAndDescription()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "activity_info", message = "Tap on the Info tab and add a title and description." });
            _mobileTutorial.Show(queue);
        }

        public void WhatIsAnActionStep()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "step_item", message = "An action step is the basic component of a learning activity created in MirageXR. Each activity consists of one or multiple action steps.", position = TutorialModel.MessagePosition.Bottom});
            _mobileTutorial.Show(queue);
        }

        public void SelectMultipleSteps()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "step_item", message = "Long tap on the action step to select it.", position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        public void RenameSteps()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "step_edit_step", message = "Tap on the edit button below the step and navigate to the Info tab where you can change the title section.", position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        public void AddContent()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "step_edit_step", message = "To add augmentations to a step click the edit button below any action step.", position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        public void CopyStep()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "step_edit_step", message = "To make a copy of a step and all its contents, tap on Edit step and then tap on the info tab.", position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }
    }
}

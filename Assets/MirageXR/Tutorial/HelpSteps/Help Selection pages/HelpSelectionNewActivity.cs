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

            _popup.CreateNewSelectionButton("How and why to calibrate").onClick.AddListener(HowDoICalibrate);
            _popup.CreateNewSelectionButton("What is an action step").onClick.AddListener(WhatIsAnActionStep);
            if (editMode)
            {
                _popup.CreateNewSelectionButton("How to change activity title and description").onClick.AddListener(ChangeTitleAndDescription);
                _popup.CreateNewSelectionButton("How can I make multiple steps").onClick.AddListener(SelectMultipleSteps);
                _popup.CreateNewSelectionButton("How to rename a step").onClick.AddListener(RenameSteps);
                _popup.CreateNewSelectionButton("How to add content to a step").onClick.AddListener(AddContent);
                //_popup.CreateNewSelectionButton("How to copy a step").onClick.AddListener(CopyStep); TODO: Add this when we can actually copy
                _popup.CreateNewSelectionButton("How to save an activity").onClick.AddListener(SaveActivity);
            }
        }

        private void HowDoICalibrate()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "activity_calibration", Message = "Calibration ensures that all locations in 3D space are correct next time you or someone else opens the activity. Each activity needs to be calibrated when editing and when viewing. To calibrate, tap the Calibration tab and start calibration." });
            _mobileTutorial.Show(queue);
        }

        private void ChangeTitleAndDescription()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "activity_info", Message = "Tap on the Info tab and add a title and description." });
            _mobileTutorial.Show(queue);
        }

        private void WhatIsAnActionStep()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "step_item", Message = "An action step is the basic component of a learning activity created in MirageXR. Each activity consists of one or multiple action steps.", Position = TutorialModel.MessagePosition.Bottom});
            _mobileTutorial.Show(queue);
        }

        private void SelectMultipleSteps()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "activity_add_step", Message = "Tap on the add step button.", Position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        private void RenameSteps()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "step_edit_step", Message = "Tap on the edit button below the step and navigate to the Info tab where you can change the title section.", Position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        private void AddContent()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "step_edit_step", Message = "To add augmentations to a step click the edit button below any action step.", Position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        private void CopyStep()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "step_edit_step", Message = "To make a copy of a step and all its contents, tap on Edit step and then tap on the info tab.", Position = TutorialModel.MessagePosition.Bottom });
            _mobileTutorial.Show(queue);
        }

        private void SaveActivity()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "activity_publish", Message = "Activities are automatically saved locally, but you need to publish them to the cloud, if you want an online backup or want to share them. You have to mark them as 'public', if you want others to see and access your activity.", Position = TutorialModel.MessagePosition.Middle });
            _mobileTutorial.Show(queue);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// The TutorialManager controls and executes the TutorialSteps,
    /// which together represent a tutorial scenario. Steps are executed
    /// in a linear, discrete and disjunct manner.
    /// </summary>
    public class TutorialManager
    {
        public static readonly string PLAYER_PREFS_STATUS_KEY = "TutorialStatus";
        public static readonly int STATUS_LOAD_ON_START = 0;
        public static readonly int STATUS_DO_NOT_LOAD_ON_START = 1;

        private static TutorialManager instance;

        /// <summary>
        /// The TutorialManager singleton.
        /// </summary>
        /// <returns></returns>
        public static TutorialManager Instance()
        {
            if (instance == null)
            {
                instance = new TutorialManager();
            }
            return instance;
        }

        /// <summary>
        /// Status field showing if the tutorial is currently running.
        /// Set internally by the TutorialManager.
        /// </summary>
        public bool IsTutorialRunning { get; private set; }

        private List<TutorialStep> steps;
        private int currentStepNumber;

        public TutorialButton TutorialButton;

        /// <summary>
        /// This string is necessary to locate the newly created label
        /// after the "AddLabelToScene" step.
        /// </summary>
        public ToggleObject CreatedLabel;

        private TutorialManager()
        {
            IsTutorialRunning = false;
            steps = new List<TutorialStep>();
        }


        /// <summary>
        /// Method that sets up and starts the tutorial.
        /// Setup includes loading and ordering the list of steps.
        /// The tutorial starts from the first step.
        /// </summary>
        public void StartTutorial()
        {
            IsTutorialRunning = true;
            if (TutorialButton != null)
            {
                TutorialButton.SetIconActive();
            }

            PopulateStepList();
            currentStepNumber = -1;
            NextStep();            
        }

        private bool PopulateStepList()
        {
            steps.Clear();
            steps.Add(new StepUnlockActivityMenu());
            steps.Add(new StepDragActivityMenu());
            steps.Add(new StepCreateNewActivity());
            steps.Add(new StepDragActionEditor());
            steps.Add(new StepRenameActivity());
            steps.Add(new StepCreateNewActionStep());
            steps.Add(new StepAddActionStepTitle());
            steps.Add(new StepAddActionStepDescription());
            steps.Add(new StepCreateNewAugmentation());
            steps.Add(new StepSelectLabelAugmentation());
            steps.Add(new StepEnterLabelText());
            steps.Add(new StepAddLabelToScene());
            steps.Add(new StepMoveCreatedLabel());
            steps.Add(new StepDeleteActionStep());
            steps.Add(new StepSaveActivity());
            steps.Add(new StepUploadActivity());
            return true;
        }

        /// <summary>
        /// Method that initiates the next TutorialStep in the list.
        /// Should be called exclusively by the previous step to make sure
        /// it has ended itself.
        /// </summary>
        public void NextStep()
        {
            currentStepNumber++;
            if (currentStepNumber < steps.Count)
            {
                steps[currentStepNumber].EnterStep();
            }
            else
            {
                EndTutorial();
            }
        }

        /// <summary>
        /// To be run if tutorial is entirely and succesfully completed.
        /// </summary>
        public void EndTutorial()
        {
            steps.Clear();
            IsTutorialRunning = false;
            if (TutorialButton != null)
            {
                TutorialButton.SetIconInactive();
            }

            PlayerPrefs.SetInt(PLAYER_PREFS_STATUS_KEY, STATUS_DO_NOT_LOAD_ON_START);
        }

        /// <summary>
        /// To be run if tutorial is closed without being fully completed,
        /// i.e. the tutorial was closed in the middle of a step. This method
        /// is also called from the TutorialSteps if they encounter an error.
        /// </summary>
        public void CloseTutorial()
        {
            steps[currentStepNumber].CloseStep();

            steps.Clear();
            IsTutorialRunning = false;
            if (TutorialButton != null)
            {
                TutorialButton.SetIconInactive();
            }

            PlayerPrefs.SetInt(PLAYER_PREFS_STATUS_KEY, STATUS_DO_NOT_LOAD_ON_START);
        }

        /// <summary>
        /// Starts the tutorial if it is currently not running and
        /// closes it otherwise.
        /// </summary>
        public void ToggleTutorial()
        {
            if (IsTutorialRunning)
            {
                CloseTutorial();
            }
            else
            {
                StartTutorial();
            }

        }
    }
}

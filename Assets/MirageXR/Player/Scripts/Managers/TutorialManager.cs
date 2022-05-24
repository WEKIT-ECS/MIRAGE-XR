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
    public class TutorialManager : MonoBehaviour
    {
        /// <summary>
        /// Status key for PlayerPrefs whether the tutorial should be started automatically.
        /// </summary>
        public const string PLAYER_PREFS_STATUS_KEY = "TutorialStatus";
        /// <summary>
        /// Status value for PlayerPrefs that the Tutorial should be started automatically.
        /// </summary>
        public const int STATUS_LOAD_ON_START = 0;
        /// <summary>
        /// Status value for PlayerPrefs that the Tutorial should not start automatically.
        /// </summary>
        public const int STATUS_DO_NOT_LOAD_ON_START = 1;

        /// <summary>
        /// Types of the Tutorial currently offered.
        /// The type depends heavily on the platform.
        /// </summary>
        public enum TutorialType
        {
            HOLOLENS,
            MOBILE_EDITING,
            MOBILE_VIEWING
        }

        /// <summary>
        /// The TutorialManager singleton Instance.
        /// </summary>
        public static TutorialManager Instance { get; private set; }

        /// <summary>
        /// Status field showing if the tutorial is currently running.
        /// Set internally by the TutorialManager.
        /// </summary>
        public bool IsTutorialRunning { get; private set; }

        private List<TutorialStep> steps;
        private int currentStepNumber;

        /// <summary>
        /// TutorialButton on the Hololens UI.
        /// </summary>
        public TutorialButton TutorialButton;

        /// <summary>
        /// This string is necessary to locate the newly created label
        /// after the "AddLabelToScene" step.
        /// </summary>
        public ToggleObject CreatedLabel;

        /// <summary>
        /// Object Highlighter used in the mobile tutorial.
        /// </summary>
        public TutorialObjectHighlighter MobileHighlighter { get; private set; }

        [SerializeField] private TutorialPopup mobilePopup;
        /// <summary>
        /// The Popup that states the instruction text of a mobile tutorial step.
        /// Based on the PopupViewer subsystem.
        /// </summary>
        public TutorialPopup MobilePopup => mobilePopup;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"{Instance.GetType().FullName} must only be a single copy!");
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            IsTutorialRunning = false;
            steps = new List<TutorialStep>();
            MobileHighlighter = new TutorialObjectHighlighter();
        }

        /// <summary>
        /// Method that sets up and starts the tutorial.
        /// Setup includes loading and ordering the list of steps.
        /// The tutorial starts from the first step.
        /// </summary>
        /// <param name="type">The type of the tutorial.
        /// Take care to give it the right type, based on the current platform
        /// </param>
        public void StartTutorial(TutorialType type)
        {
            if (type == TutorialType.HOLOLENS)
            {
                IsTutorialRunning = true;
                if (TutorialButton != null)
                {
                    TutorialButton.SetIconActive();
                }

                PopulateStepListForHololens();
                currentStepNumber = -1;

                NextStep();
            }
            else if (type == TutorialType.MOBILE_EDITING)
            {
                IsTutorialRunning = true;

                PopulateStepListForMobileEditing();
                currentStepNumber = -1;

                NextStep();
            }
            else if (type == TutorialType.MOBILE_VIEWING)
            {
                IsTutorialRunning = true;

                PopulateStepListForMobileViewing();
                currentStepNumber = -1;

                NextStep();
            }
            else
            {
                Debug.LogError("Tried to start unknown tutorial type.");
            }    
        }

        private void PopulateStepListForHololens()
        {
            steps.Clear();
            steps.Add(new StepUnlockActivityMenu());
            steps.Add(new StepDragActivityMenu());
            steps.Add(new StepLockActivityMenu());
            steps.Add(new StepCreateNewActivity());
            steps.Add(new StepDragActionEditor());
            steps.Add(new StepRenameActivity());
            steps.Add(new StepCreateNewActionStep());
            steps.Add(new StepAddActionStepTitle());
            steps.Add(new StepAddActionStepDescription());
            //steps.Add(new StepCreateNewAugmentation());
            steps.Add(new StepSelectLabelAugmentation());
            steps.Add(new StepEnterLabelText());
            steps.Add(new StepAddLabelToScene());
            steps.Add(new StepMoveCreatedLabel());
            steps.Add(new StepDeleteActionStep());
            steps.Add(new StepSaveActivity());
            steps.Add(new StepUploadActivity());
        }

        private void PopulateStepListForMobileEditing()
        {
            steps.Clear();
            steps.Add(new MobileStepCreateActivity());
            steps.Add(new MobileStepClickActivityInfo());
            steps.Add(new WaitForMobilePageChangeStep());
            steps.Add(new MobileStepAddActivityName());
            steps.Add(new MobileStepCreateActionStep());
            steps.Add(new MobileStepClickActionStepDetails());
            steps.Add(new WaitForMobilePageChangeStep());
            steps.Add(new MobileStepAddActionStepTitle());
            steps.Add(new MobileStepExpandStepDetails());
            steps.Add(new MobileStepAddActionStepDescription());
            steps.Add(new MobileStepClickAddStepContent());
            string message = "This concludes the tutorial! From here " +
                "you can choose and add different types of augmentations. " +
                "Have fun trying them all out.";
            steps.Add(new MobileOnlyDialogStep(message));
        }

        private void PopulateStepListForMobileViewing()
        {
            steps.Clear();
            steps.Add(new MobileOnlyDialogStep("Coming soon!"));
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
        public void ToggleTutorial(TutorialType type)
        {
            if (IsTutorialRunning)
            {
                CloseTutorial();
            }
            else
            {
                StartTutorial(type);
            }

        }
    }
}

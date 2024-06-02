using i5.Toolkit.Core.VerboseLogging;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private List<TutorialStep> _steps;
        private int _currentStepNumber;

        public TutorialUI MobileTutorial { get; private set; }
        private bool _isInEditMode;

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

        [SerializeField] private HelpSelectionPopup _helpSelectionPopup;
        /// <summary>
        /// The Popup that states the instruction text of a mobile tutorial step.
        /// Based on the PopupViewer subsystem.
        /// </summary>
        public HelpSelectionPopup HelpSelectionPopup => _helpSelectionPopup;

        [SerializeField] private HelpPopup _helpPopup;
        /// <summary>
        /// The Popup that states the instruction text of a mobile tutorial step.
        /// Based on the PopupViewer subsystem.
        /// </summary>
        public HelpPopup HelpPopup => _helpPopup;

        [SerializeField] private TutorialPopup _mobilePopup;
        /// <summary>
        /// The Popup that states the instruction text of a mobile tutorial step.
        /// Based on the PopupViewer subsystem.
        /// </summary>
        public TutorialPopup MobilePopup => _mobilePopup;

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
            _steps = new List<TutorialStep>();
            MobileHighlighter = new TutorialObjectHighlighter();
            EventManager.OnEditModeChanged += EditModeListener;
        }

        private void EditModeListener(bool value)
        {
            _isInEditMode = value;
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
                _currentStepNumber = -1;

                NextStep();
            }
            else if (type == TutorialType.MOBILE_EDITING)
            {
                IsTutorialRunning = true;

                PopulateStepListForMobileEditing();

                _currentStepNumber = -1;

                NextStep();
            }
            else if (type == TutorialType.MOBILE_VIEWING)
            {
                IsTutorialRunning = true;

                PopulateStepListForMobileViewing();
                _currentStepNumber = -1;

                NextStep();
            }
            else
            {
                Debug.LogError("Tried to start unknown tutorial type.");
            }
        }

        private void PopulateStepListForHololens()
        {
            _steps.Clear();
            _steps.Add(new StepUnlockActivityMenu());
            _steps.Add(new StepDragActivityMenu());
            _steps.Add(new StepLockActivityMenu());
            _steps.Add(new StepCreateNewActivity());
            _steps.Add(new StepDragActionEditor());
            _steps.Add(new StepRenameActivity());
            _steps.Add(new StepCreateNewActionStep());
            _steps.Add(new StepAddActionStepTitle());
            _steps.Add(new StepAddActionStepDescription());
            //steps.Add(new StepCreateNewAugmentation());
            _steps.Add(new StepSelectLabelAugmentation());
            _steps.Add(new StepEnterLabelText());
            _steps.Add(new StepAddLabelToScene());
            _steps.Add(new StepMoveCreatedLabel());
            _steps.Add(new StepDeleteActionStep());
            _steps.Add(new StepSaveActivity());
            _steps.Add(new StepUploadActivity());
        }

        private void PopulateStepListForMobileEditing()
        {
            _steps.Clear();
            _steps.Add(new MobileStepCreateActivity());
            _steps.Add(new MobileStepClickActivityInfo());
            _steps.Add(new WaitForMobilePageChangeStep());
            _steps.Add(new MobileStepAddActivityName());
            _steps.Add(new MobileStepCreateActionStep());
            _steps.Add(new MobileStepClickActionStepDetails());
            _steps.Add(new WaitForMobilePageChangeStep());
            _steps.Add(new MobileStepAddActionStepTitle());
            _steps.Add(new MobileStepExpandStepDetails());
            _steps.Add(new MobileStepAddActionStepDescription());
            _steps.Add(new MobileStepClickAddStepContent());
            string message = "This concludes the tutorial! From here " +
                "you can choose and add different types of augmentations. " +
                "Have fun trying them all out.";
            _steps.Add(new MobileOnlyDialogStep(message));
        }


        private void PopulateStepListForMobileViewing()
        {
            _steps.Clear();
            _steps.Add(new MVTSelectTutorialActivityStep());
            _steps.Add(new MVTCalibrationGuideStep());
            _steps.Add(new MVTSwitchTabsStep());
            _steps.Add(new MVTLabelTriggerStep());
            _steps.Add(new MVTGhostTrackStep());
            _steps.Add(new MVTHighlightTriggerStep());
            _steps.Add(new MVTPickAndPlaceStep());
            _steps.Add(new MVTFinishTutorialStep());
        }

        /// <summary>
        /// Method that initiates the next TutorialStep in the list.
        /// Should be called exclusively by the previous step to make sure
        /// it has ended itself.
        /// </summary>
        public void NextStep()
        {
            _currentStepNumber++;
            if (_currentStepNumber < _steps.Count)
            {
                _steps[_currentStepNumber].EnterStep();
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
            _steps.Clear();
            IsTutorialRunning = false;
            if (TutorialButton != null)
            {
                TutorialButton.SetIconInactive();
                PlayerPrefs.SetInt(PLAYER_PREFS_STATUS_KEY, STATUS_DO_NOT_LOAD_ON_START);
            }
        }

        /// <summary>
        /// To be run if tutorial is closed without being fully completed,
        /// i.e. the tutorial was closed in the middle of a step. This method
        /// is also called from the TutorialSteps if they encounter an error.
        /// </summary>
        public void CloseTutorial()
        {
            if (IsTutorialRunning)
            {
                _steps[_currentStepNumber].CloseStep();

                _steps.Clear();
                IsTutorialRunning = false;
                if (TutorialButton != null)
                {
                    TutorialButton.SetIconInactive();
                    PlayerPrefs.SetInt(PLAYER_PREFS_STATUS_KEY, STATUS_DO_NOT_LOAD_ON_START);
                }
            }
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

        /// <summary>
        /// Shows the help selection popup, which is dynamically generated
        /// for each UI page.
        /// </summary>
        /// <param name="page">Page for which to show help, probably current page.</param>
        public void ShowHelpSelection(RootView_v2.HelpPage page)
        {
            if (MobileTutorial == null)
            {
                MobileTutorial = RootView_v2.Instance.Tutorial;
            }

            var popup = (HelpSelectionPopup)PopupsViewer.Instance.Show(HelpSelectionPopup);
            bool isEditModeOn = RootObject.Instance.activityManager.EditModeActive;

            popup.LoadHelpSelection(page, isEditModeOn);
        }

        /// <summary>
        /// Starts the mobile editing tutorial using the UI-based system.
        /// </summary>
        public void StartNewMobileEditingTutorial()
        {
            if (MobileTutorial == null)
            {
                MobileTutorial = RootView_v2.Instance.Tutorial;
            }

            var queue = new Queue<TutorialModelUI>();
            queue.Enqueue(new TutorialModelUI { Id = "activity_create", Message = "Welcome to the MirageXR editing tutorial! To start, let's create a new activity by tapping the plus button below.", BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "activity_info", Message = "We should add some info about our activity so it's recognisable. To do this, tap the Info tab.", Position = TutorialModelUI.MessagePosition.Middle, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "activity_title", Message = "To give our activity a new title, we can tap on the field below.", Position = TutorialModelUI.MessagePosition.Middle, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "activity_description", Message = "Activity descriptions help users understand what an activity is about. To add one, we can tap on the field below.", Position = TutorialModelUI.MessagePosition.Middle, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "activity_steps", Message = "Now we're going to add some steps to our activity. Tap the Steps tab to continue.", Position = TutorialModelUI.MessagePosition.Middle, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "activity_add_step", Message = "Activities consist of steps, which hold content for users to experience. Let's create a new step by tapping the plus button below.", Position = TutorialModelUI.MessagePosition.Top, BtnText = "Skip" });
            //queue.Enqueue(new TutorialModel { id = "activity_edit_step", message = "Empty steps aren't really entertaining. Let's add some content to our step by tapping the Edit Step button.", position = TutorialModel.MessagePosition.Bottom, btnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "step_info", Message = "First let's name and describe our step so users know what to expect. Tap the Info tab to continue.", Position = TutorialModelUI.MessagePosition.Middle, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "step_title", Message = "Just like with the Activity, we should add a title...", Position = TutorialModelUI.MessagePosition.Bottom, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "step_description", Message = "...and a description to our step.", Position = TutorialModelUI.MessagePosition.Bottom, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "step_augmentations", Message = "Finally, let's add some content to our Step. To do so, tap the Augmentations tab.", Position = TutorialModelUI.MessagePosition.Middle, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Id = "step_add_augmentation", Message = "Augmentations represent different AR content for our users. A list of possible augmentations can be seen by tapping the plus button.", Position = TutorialModelUI.MessagePosition.Bottom, BtnText = "Skip" });
            queue.Enqueue(new TutorialModelUI { Message = "Here you can choose any of the available augmentations to add to the step. More information on each augmentation is available on their info page. This concludes the tutorial, have fun exploring!", Position = TutorialModelUI.MessagePosition.Middle, BtnText = "Got it" });
            MobileTutorial.Show(queue);
        }

        /// <summary>
        /// Starts the mobile viewing tutorial, which is a mix of the
        /// UI-based system and the world-space one. It needs to be async
        /// because certain parts are delayed.
        /// </summary>
        public async void StartNewMobileViewingTutorial()
        {
            if (MobileTutorial == null)
            {
                MobileTutorial = RootView_v2.Instance.Tutorial;
            }

            ActivityListView_v2 alv = RootView_v2.Instance.activityListView;
            await alv.CreateTutorialActivity();
            await Task.Delay(1000);

            StartTutorial(TutorialType.MOBILE_VIEWING);
        }
    }
}

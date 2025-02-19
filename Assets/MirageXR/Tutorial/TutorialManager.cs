using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;
using System;
using UnityEngine.Android;

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

        private const string FILE_NAME_TUTORIAL_MOBILE_VIEWING = "tutorialMobileViewing";
        private const string FILE_NAME_TUTORIAL_MOBILE_EDITING = "tutorialMobileEditing";
        private const string FILE_NAME_TUTORIAL_MOBILE_AUGMENTATION = "AugmentationTutorials/tutorialAugmentation_{0}";

        /// <summary>
        /// Types of the Tutorial currently offered.
        /// The type depends heavily on the platform.
        /// </summary>
        public enum TutorialType
        {
            HOLOLENS,
            MOBILE_EDITING,
            MOBILE_VIEWING,
            MOBILE_AUGMENTATION
        }

        public enum TutorialEvent
        {
            NON_EVENT,
            UI_FINISHED_QUEUE,
            UI_GOT_IT,
            CALIBRATION_FINISHED,
            ACTION_STEP_ACTIVATED,
            PICK_AND_PLACED,
            GHOST_REPLAYED,
            EDIT_MODE_CHANGED,
            PICK_POSITION_CHANGED,
            TARGET_POSITION_CHANGED,
            VIDEO_SELECTED_FROM_GALLERY
        }

        public enum TutorialExitCode
        {
            FINISHED = 0,
            // Intended exits
            USER_EXIT = 101,
            PREDEFINED_EXIT = 102,
            // Step-related errors
            STEP_TYPE_UNKNOWN = 201,
            STEP_INVALID = 202,
            STEP_NO_SHOW = 203,
            // Event-related errors
            NON_EVENT_INVOKED = 301,
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
        // TODO: put this as a derived value from if _currentTutorial is null or not

        private List<TutorialStep> _steps;
        private int _currentStepNumber;

        private TutorialModel _currentTutorial;
        private int _newCurrentStepNumber;

        public TutorialHandlerUI MobileTutorial => RootView_v2.Instance.Tutorial;
        private bool _isInEditMode;

        private TutorialHandlerWS _handlerWS;

        /// <summary>
        /// TutorialButton on the Hololens UI.
        /// </summary>
        public TutorialButton TutorialButton;

        /// <summary>
        /// This string is necessary to locate the newly created label
        /// after the "AddLabelToScene" step.
        /// </summary>
        public LearningExperienceEngine.ToggleObject CreatedLabel;

        [SerializeField] private HelpSelectionPopup _helpSelectionPopup;
        /// <summary>
        /// The Popup that states the instruction text of a mobile tutorial step.
        /// Based on the PopupViewer subsystem.
        /// </summary>
        public HelpSelectionPopup HelpSelectionPopup => _helpSelectionPopup;

        private TutorialEvent _expectedEvent = TutorialEvent.NON_EVENT;
        private List<TutorialEvent> _currentClosingEvents = new List<TutorialEvent>();

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
            _handlerWS = new TutorialHandlerWS();
            _currentClosingEvents = new List<TutorialEvent>();
            LearningExperienceEngine.EventManager.OnEditModeChanged += EditModeListener;
            LearningExperienceEngine.EventManager.OnActivateAction += StepActivatedListener;
        }

        private void EditModeListener(bool value)
        {
            _isInEditMode = value;
            InvokeEvent(TutorialEvent.EDIT_MODE_CHANGED);
        }

        private void StepActivatedListener(string action)
        {
            InvokeEvent(TutorialEvent.ACTION_STEP_ACTIVATED);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////// MOSTLY LEGACY, WILL BE REMOVED WHEN REFACTORING HOLOLENS TUTORIAL /////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////// END OF LEGACY /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Shows the help selection popup, which is dynamically generated
        /// for each UI page.
        /// </summary>
        /// <param name="page">Page for which to show help, probably current page.</param>
        public void ShowHelpSelection(RootView_v2.HelpPage page)
        {
            var popup = (HelpSelectionPopup)PopupsViewer.Instance.Show(HelpSelectionPopup);
            bool isEditModeOn = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive;

            popup.LoadHelpSelection(page, isEditModeOn);
        }

        /// <summary>
        /// Starts the mobile editing tutorial using the UI-based system.
        /// </summary>
        public void StartNewMobileEditingTutorial()
        {
            NewStartTutorial(TutorialType.MOBILE_EDITING);
        }

        /// <summary>
        /// Starts the mobile viewing tutorial, which is a mix of the
        /// UI-based system and the world-space one. It needs to be async
        /// because certain parts are delayed.
        /// </summary>
        public async void StartNewMobileViewingTutorial()
        {
            ActivityListView_v2 alv = RootView_v2.Instance.activityListView;
            await alv.CreateTutorialActivity();
            await Task.Delay(1000);

            // Here it is necessary that the Tutorial Activity is the first in the list
            ActivityListItem_v2 tutorialActivityCard = alv.GetComponentsInChildren<ActivityListItem_v2>()[0];

            // Add TutorialItem to the dynamically created activity UI element
            TutorialItem titem = tutorialActivityCard.gameObject.AddComponent(typeof(TutorialItem)) as TutorialItem;
            titem.SetId("tutorial_activity");
            titem.SetInteractableObject(tutorialActivityCard.gameObject);

            NewStartTutorial(TutorialType.MOBILE_VIEWING);
        }


        /// <summary>
        /// Handles tutorials through the JSON deserialising system.
        /// </summary>
        /// <param name="type">Type of the tutorial to be started.</param>
        /// <param name="option">Additional information for the type.</param>
        public void NewStartTutorial(TutorialType type, string option = null)
        {
            if (_currentTutorial != null)
            {
                // TODO: Perhaps indicate to the user that a tutorial is already running
                ExitTutorial(TutorialExitCode.USER_EXIT);
                return;
            }

            string neededFile = "";
            switch (type)
            {
                case TutorialType.MOBILE_VIEWING:
                    neededFile = FILE_NAME_TUTORIAL_MOBILE_VIEWING;
                    break;
                case TutorialType.MOBILE_EDITING:
                    neededFile = FILE_NAME_TUTORIAL_MOBILE_EDITING;
                    break;
                case TutorialType.MOBILE_AUGMENTATION:
                    neededFile = string.Format(FILE_NAME_TUTORIAL_MOBILE_AUGMENTATION, option);
                    break;
                    // TODO: put others here as well
            }

            if (neededFile == "")
            {
                Debug.LogError("Requested to start unknown tutorial in TutorialManager.");
                return;
            }

            try
            {
                TextAsset jsonFile = Resources.Load<TextAsset>(neededFile);
                TutorialModel tmodel = JsonConvert.DeserializeObject<TutorialModel>(jsonFile.text);
                tmodel.PopulateParentReferences();

                _currentTutorial = tmodel;
                _newCurrentStepNumber = -1;
                IsTutorialRunning = true;

                NewNextStep();
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError("File not found while loading tutorial: " + e.FileName);
            }
            catch (JsonException e)
            {
                Debug.LogError("JSON parsing error while loading tutorial: " + e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError("An unexpected error occurred while loading tutorial: " + e.Message);
            }

        }

        private void NewNextStep()
        {
            if (_currentTutorial == null)
            {
                Debug.LogError("NextStep requested without running tutorial in TutorialManager.");
                return;
            }

            // Set to next element
            _newCurrentStepNumber++;

            if (_newCurrentStepNumber < _currentTutorial.Steps.Count)
            {
                TutorialStepModel currentStep = _currentTutorial.Steps[_newCurrentStepNumber];
                // Coroutine needed to process delays
                StartCoroutine(HandleStepWithDelay(currentStep));
            }
            else
            {
                ExitTutorial(TutorialExitCode.FINISHED);
            }
        }

        /// <summary>
        /// Task which handles steps. Sometimes necessary to delay, e.g. when
        /// items are still loading into the scene.
        /// </summary>
        /// <param name="currentStep">The step to be processed.</param>
        private IEnumerator HandleStepWithDelay(TutorialStepModel currentStep)
        {
            // Check if the step has a delay
            if (currentStep.DelayInMilliseconds > 0)
            {
                // Convert to delay in seconds
                yield return new WaitForSeconds(currentStep.DelayInMilliseconds / 1000f);
            }

            // Process steps based on type
            switch (currentStep)
            {
                case TutorialStepModelUI uiStep:
                    HandleUIStep(uiStep);
                    break;

                case TutorialStepModelWS wsStep:
                    HandleWSStep(wsStep);
                    break;

                case TutorialStepModelEO eoStep:
                    HandleEOStep(eoStep);
                    break;

                default:
                    Debug.LogError("Unknown step type in TutorialManager.");
                    ExitTutorial(TutorialExitCode.STEP_TYPE_UNKNOWN);
                    break;
            }
        }

        private void HandleUIStep(TutorialStepModelUI uiStep)
        {
            if (!uiStep.IsValid())
            {
                Debug.LogError("UIStep is not valid in TutorialManager.");
                ExitTutorial(TutorialExitCode.STEP_INVALID);
                return;
            }

            // Set finish event
            _expectedEvent = TutorialEvent.UI_FINISHED_QUEUE;

            // Show by UI step handler
            var queue = new Queue<TutorialStepModelUI>();
            queue.Enqueue(uiStep);
            MobileTutorial.Show(queue);
        }

        private void HandleWSStep(TutorialStepModelWS wsStep)
        {
            if (!wsStep.IsValid())
            {
                Debug.LogError("WSStep is not valid in TutorialManager.");
                ExitTutorial(TutorialExitCode.STEP_INVALID);
                return;
            }

            if (wsStep.FocusObject == "<<LAST_AUGMENTATION_CREATED>>")
            {
                wsStep.FocusObject = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.GetLastCreatedAugmentationId();
            }

            // Show by WS step handler
            bool success = _handlerWS.Show(wsStep);
            if (!success)
            {
                Debug.LogError("Unable to show WS Step in Tutorial Manager.");
                ExitTutorial(TutorialExitCode.STEP_NO_SHOW);
            }

            _expectedEvent = wsStep.FinishEvent;
            _currentClosingEvents = wsStep.CloseEvents;
        }

        private void HandleEOStep(TutorialStepModelEO eoStep)
        {
            if (!eoStep.IsValid())
            {
                Debug.LogError("EOStep is not valid in TutorialManager.");
                ExitTutorial(TutorialExitCode.STEP_INVALID);
                return;
            }

            _expectedEvent = eoStep.FinishEvent;
            _currentClosingEvents = eoStep.CloseEvents;
        }

        /// <summary>
        /// Does clean-up actions on tutorial exit.
        /// </summary>
        /// <param name="code">Indicates why the tutorial exited.</param>
        private void ExitTutorial(TutorialExitCode code)
        {
            _handlerWS.Hide();
            IsTutorialRunning = false;
            _currentTutorial = null;

            // This means the tutorial finished successfully
            if (code == TutorialExitCode.FINISHED)
            {
                Debug.LogDebug("Tutorial exited successfully!");
            }
            else
            {
                Debug.LogDebug("Tutorial closed due to error code: " + code);
            }
        }

        /// <summary>
        /// Method that serves as the TutorialManager's event handler. It should be called to indicate
        /// that events have occured, which serve as tutorial steps' finish events.
        /// Effectively also serves as EndEvent for dealing with logic that should happen on step end.
        /// </summary>
        /// <param name="tevent">The event that has occured.</param>
        /// <returns>True if this was the expected event to advance the tutorial or not.</returns>
        public bool InvokeEvent(TutorialEvent tevent)
        {
            if (_currentTutorial == null)
            {
                return false;
            }

            if (tevent == TutorialEvent.NON_EVENT)
            {
                Debug.LogError("TutorialManager received invocation of NON_EVENT, this should not happen.");
                ExitTutorial(TutorialExitCode.NON_EVENT_INVOKED);
                return false;
            }

            if (tevent == TutorialEvent.UI_GOT_IT)
            {
                Debug.LogDebug("TutorialManager closing due to Got It pressed in UI tutorial handler.");
                ExitTutorial(TutorialExitCode.USER_EXIT);
                return false;
            }

            if (tevent == _expectedEvent)
            {
                // Hide any existing elements
                _handlerWS.Hide();
                NewNextStep();
                return true;
            }

            if (_currentClosingEvents.Contains(tevent))
            {
                Debug.LogDebug("TutorialManager closing due to predefined closing event.");
                ExitTutorial(TutorialExitCode.PREDEFINED_EXIT);
                return false;
            }

            return false;
        }

        public void StartAugmentationTutorial(LearningExperienceEngine.ContentType contentType)
        {
            Debug.LogDebug("Starting Augmentation Tutorial: " + LearningExperienceEngine.ContentTypeExtension.GetName(contentType));
            NewStartTutorial(TutorialType.MOBILE_AUGMENTATION, LearningExperienceEngine.ContentTypeExtension.GetName(contentType));
        }
    }
}

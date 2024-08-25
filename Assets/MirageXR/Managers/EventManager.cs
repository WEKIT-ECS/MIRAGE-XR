using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Event manager for handling communication between Activity manager
    /// and Workplace manager.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        
        //public delegate void AppSelectionExitDelegate();
        //public static event AppSelectionExitDelegate OnAppSelectionExit;
        ///// <summary>
        ///// MirageXR app selection exit.
        ///// </summary>
        //public static void AppSelectionExit()
        //{
        //    OnAppSelectionExit?.Invoke();
        //}

        // MirageXR recorder exit.
        public delegate void RecorderExitDelegate();
        public static event RecorderExitDelegate OnRecorderExit;
        public static void RecorderExit()
        {
            OnRecorderExit?.Invoke();
        }

        // MirageXR player exit.
        public delegate void PlayerExitDelegate();
        public static event PlayerExitDelegate OnPlayerExit;
        public static void PlayerExit()
        {
            OnPlayerExit?.Invoke();
        }


        //// LEGACY: this is for displaying the Debug Log as text in an assigned GameObject
        //public delegate void DebugLogDelegate(string debug);
        //public static event DebugLogDelegate OnDebugLog;
        ///// <summary>
        ///// Adds debug message to UI debug console and normal debug log.
        ///// </summary>
        ///// <param name="debug">Debug message.</param>
        //public static void DebugLog(string debug)
        //{
        //    Debug.LogInfo(debug);
        //    OnDebugLog?.Invoke(debug);
        //}

        // MirageXR move activity list.
        public delegate void MoveActivityDelegate();
        public static event MoveActivityDelegate OnMoveActivityList;
        /// <summary>
        /// Move MirageXR activity list.
        /// </summary>
        public static void MoveActivityList()
        {
            OnMoveActivityList?.Invoke();
        }

        // MirageXR move action list.
        public delegate void MoveActionDelegate();
        public static event MoveActionDelegate OnMoveActionList;
        /// <summary>
        /// Move MirageXR activity list.
        /// </summary>
        public static void MoveActionList()
        {
            OnMoveActionList?.Invoke();
        }

        public delegate void ShowGuidesDelegate();
        public static event ShowGuidesDelegate OnShowGuides;
        /// <summary>
        /// Show guide lines.
        /// </summary>
        public static void ShowGuides()
        {
            OnShowGuides?.Invoke();
        }

        public delegate void HideGuidesDelegate();
        public static event HideGuidesDelegate OnHideGuides;
        /// <summary>
        /// Hide guide lines.
        /// </summary>
        public static void HideGuides()
        {
            OnHideGuides?.Invoke();
        }

        public delegate void ToggleGuidesDelegate();
        public static event ToggleGuidesDelegate OnToggleGuides;
        public static void ToggleGuides()
        {
            OnToggleGuides?.Invoke();
        }

        public delegate void ToggleMenuDelegate();
        public static event ToggleMenuDelegate OnToggleMenu;
        public static void ToggleMenu()
        {
            OnToggleMenu?.Invoke();
        }

        public delegate void ToggleLockDelegate();
        public static event ToggleLockDelegate OnToggleLock;
        public static void ToggleLock()
        {
            OnToggleLock?.Invoke();
        }

        // - - - - - - - - - - - - - - - - - - - -  Voice Events
        // Player UI related events. Needed for the unified KeywordManager.

        //TODO: Replace "voice" events with a single event with an enum parameter 
        // For opening the augmentation list
        public delegate void OpenAnnotationByVoiceDelegate();
        public static event OpenAnnotationByVoiceDelegate OnOpenAnnotationByVoice;
        public static void OpenAnnotationByVoice()
        {
            OnOpenAnnotationByVoice?.Invoke();
        }

        // For deleting the active action step
        public delegate void DeleteActionByVoiceDelegate();
        public static event DeleteActionByVoiceDelegate OnDeleteActionByVoice;
        public static void DeleteActionByVoice()
        {
            OnDeleteActionByVoice?.Invoke();
        }

        // For adding a new action step
        public delegate void AddActionByVoiceDelegate();
        public static event AddActionByVoiceDelegate OnAddActionByVoice;
        public static void AddActionByVoice()
        {
            OnAddActionByVoice?.Invoke();
        }

        // For opening login page
        public delegate void LoginByVoiceDelegate();
        public static event LoginByVoiceDelegate OnLoginByVoice;
        public static void LoginByVoice()
        {
            OnLoginByVoice?.Invoke();
        }

        // For opening Moodle register page
        public delegate void RegisterByVoiceDelegate();
        public static event RegisterByVoiceDelegate OnRegisterActionByVoice;
        public static void RegisterByVoice()
        {
            OnRegisterActionByVoice?.Invoke();
        }

        // Upload the active activity
        public delegate void UploadActivityByVoiceDelegate();
        public static event UploadActivityByVoiceDelegate OnUploadActivityActionByVoice;
        public static void UploadActivityByVoice()
        {
            OnUploadActivityActionByVoice?.Invoke();
        }

        // For showing action list by voice command.
        public delegate void ActionlistToggleByVoiceDelegate(bool status);
        public static event ActionlistToggleByVoiceDelegate OnActionlistToggleByVoice;
        public static void ActionlistToggleByVoice(bool status)
        {
            OnActionlistToggleByVoice?.Invoke(status);
        }

        // Go to next step with voice command.
        public delegate void NextByVoiceDelegate();
        public static event NextByVoiceDelegate OnNextByVoice;
        public static void NextByVoice()
        {
            OnNextByVoice?.Invoke();
        }

        // Go to previous step with voice command.
        public delegate void BackByVoiceDelegate();
        public static event BackByVoiceDelegate OnBackByVoice;
        public static void BackByVoice()
        {
            OnBackByVoice?.Invoke();
        }

        // Lock menu in place.
        public delegate void LockMenuByVoiceDelegate();
        public static event LockMenuByVoiceDelegate OnLockMenuByVoice;
        public static void LockMenuByVoice()
        {
            OnLockMenuByVoice?.Invoke();
        }

        // Release menu lock.
        public delegate void ReleaseMenuByVoiceDelegate();
        public static event ReleaseMenuByVoiceDelegate OnReleaseMenuByVoice;
        public static void ReleaseMenuByVoice()
        {
            OnReleaseMenuByVoice?.Invoke();
        }

        // Start by voice.
        public delegate void StartByVoiceDelegate();
        public static event StartByVoiceDelegate OnStartByVoice;
        public static void StartByVoice()
        {
            OnStartByVoice?.Invoke();
        }

        // - - - - - - - - - - - - - - - - - - - -  End of Voice Events


        // Start extended tracking.
        public delegate void StartExtendedTrackingDelegate();
        public static event StartExtendedTrackingDelegate OnStartExtendedTracking;
        public static void StartExtendedTracking()
        {
            OnStartExtendedTracking?.Invoke();
        }

        // Stop extended tracking.
        public delegate void StopExtendedTrackingDelegate();
        public static event StopExtendedTrackingDelegate OnStopExtendedTracking;
        public static void StopExtendedTracking()
        {
            OnStopExtendedTracking?.Invoke();
        }

        public delegate void ShowActivitySelectionMenuDelegate();
        public static event ShowActivitySelectionMenuDelegate OnShowActivitySelectionMenu;
        public static void ShowActivitySelectionMenu()
        {
            OnShowActivitySelectionMenu?.Invoke();
        }

        public delegate void HideActivitySelectionMenuDelegate();
        public static event HideActivitySelectionMenuDelegate OnHideActivitySelectionMenu;
        public static void HideActivitySelectionMenu()
        {
            OnHideActivitySelectionMenu?.Invoke();
        }

        public delegate void EditorLoadedDelegate();
        public static event EditorLoadedDelegate OnEditorLoaded;
        public static void NotifyEditorLoaded()
        {
            OnEditorLoaded?.Invoke();
        }

        public delegate void EditorUnloadedDelegate();
        public static event EditorUnloadedDelegate OnEditorUnloaded;

        public static void NotifyEditorUnloaded()
        {
            OnEditorUnloaded?.Invoke();
        }

        public delegate void ActivitySelectionMenuLockClickedDelegate();
        public static event ActivitySelectionMenuLockClickedDelegate ActivitySelectionMenuLockClicked;
        public static void NotifyOnActivitySelectionMenuLockClicked()
        {
            ActivitySelectionMenuLockClicked?.Invoke();
        }

        public delegate void ActivitySelectionMenuDragEndDelegate();
        public static event ActivitySelectionMenuDragEndDelegate ActivitySelectionMenuDragEnd;
        public static void NotifyOnActivitySelectionMenuDragEnd()
        {
            ActivitySelectionMenuDragEnd?.Invoke();
        }

        public delegate void NewActivityCreationButtonPressedDelegate();
        public static event NewActivityCreationButtonPressedDelegate NewActivityCreationButtonPressed;
        public static void NotifyOnNewActivityCreationButtonPressed()
        {
            NewActivityCreationButtonPressed?.Invoke();
        }

        public delegate void TaskStationEditorDragEndDelegate();
        public static event TaskStationEditorDragEndDelegate TaskStationEditorDragEnd;
        public static void NotifyOnTaskStationEditorDragEnd()
        {
            TaskStationEditorDragEnd?.Invoke();
        }

        public delegate void TaskStationEditorEnabledDelegate();
        public static event TaskStationEditorEnabledDelegate TaskStationEditorEnabled;
        public static void NotifyOnTaskStationEditorEnabled()
        {
            TaskStationEditorEnabled?.Invoke();
        }

        public delegate void AddAugmentationButtonClickedDelegate();
        public static event AddAugmentationButtonClickedDelegate AddAugmentationButtonClicked;
        public static void NotifyOnAddAugmentationButtonClicked()
        {
            AddAugmentationButtonClicked?.Invoke();
        }

        public delegate void LabelEditorTextChangedDelegate();
        public static event LabelEditorTextChangedDelegate LabelEditorTextChanged;
        public static void NotifyOnLabelEditorTextChanged()
        {
            LabelEditorTextChanged?.Invoke();
        }

        public delegate void ActivitySaveButtonClickedDelegate();
        public static event ActivitySaveButtonClickedDelegate ActivitySaveButtonClicked;
        public static void NotifyOnActivitySaveButtonClicked()
        {
            ActivitySaveButtonClicked?.Invoke();
        }

        public delegate void ActivityUploadButtonClickedDelegate();
        public static event ActivityUploadButtonClickedDelegate ActivityUploadButtonClicked;
        public static void NotifyOnActivityUploadButtonClicked()
        {
            ActivityUploadButtonClicked?.Invoke();
        }

        public delegate void StepsSelectorClickedDelegate();
        public static event StepsSelectorClickedDelegate StepsSelectorClicked;
        public static void NotifyOnStepsSelectorClicked()
        {
            StepsSelectorClicked?.Invoke();
        }

        public delegate void ViewSelectorClickedDelegate();
        public static event ViewSelectorClickedDelegate ViewSelectorClicked;
        public static void NotifyOnViewSelectorClicked()
        {
            ViewSelectorClicked?.Invoke();
        }

        public delegate void MobilePageChangeFinishedDelegate();
        public static event MobilePageChangeFinishedDelegate MobilePageChanged;
        public static void NotifyOnMobilePageChanged()
        {
            MobilePageChanged?.Invoke();
        }

        public delegate void TutorialPopupCloseClickedDelegate();
        public static event TutorialPopupCloseClickedDelegate TutorialPopupCloseClicked;
        public static void NotifyOnTutorialPopupCloseClicked()
        {
            TutorialPopupCloseClicked?.Invoke();
        }

        public delegate void MobileStepContentExpandedDelegate();
        public static event MobileStepContentExpandedDelegate MobileStepContentExpanded;
        public static void NotifyOnMobileStepContentExpanded()
        {
            MobileStepContentExpanded?.Invoke();
        }

        public delegate void HighlightingButtonClickedDelegate();
        public static event HighlightingButtonClickedDelegate HighlightingButtonClicked;
        public static void NotifyOnHighlightingButtonClicked()
        {
            HighlightingButtonClicked?.Invoke();
        }

        public delegate void MobileAddStepContentPressedDelegate();
        public static event MobileAddStepContentPressedDelegate MobileAddStepContentPressed;
        public static void NotifyOnMobileAddStepContentPressed()
        {
            MobileAddStepContentPressed?.Invoke();
        }

        public delegate void xAPIChangedDelegate(LearningExperienceEngine.DBManager.LearningRecordStores option);
        public static event xAPIChangedDelegate XAPIChanged;
        public static void NotifyxAPIChanged(LearningExperienceEngine.DBManager.LearningRecordStores option)
        {
            XAPIChanged?.Invoke(option);
        }

        public delegate void MoodleDomainChangedDelegate();
        public static event MoodleDomainChangedDelegate MoodleDomainChanged;
        public static void NotifyMoodleDomainChanged()
        {
            MoodleDomainChanged?.Invoke();
        }

        public delegate void MobileHelpPageChanged(RootView_v2.HelpPage value);
        public static event MobileHelpPageChanged OnMobileHelpPageChanged;
        public static void NotifyMobileHelpPageChanged(RootView_v2.HelpPage value)
        {
            OnMobileHelpPageChanged?.Invoke(value);
        }

        public delegate void PickPlacedCorrectly();
        public static event PickPlacedCorrectly OnPickPlacedCorrectly;
        /// <summary>
        /// Event fires when the user placed the Pick&Place object in the correct target location.
        /// </summary>
        public static void NotifyOnPickPlacedCorrectly()
        {
            OnPickPlacedCorrectly?.Invoke();
        }

    }
}
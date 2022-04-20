using System;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Event manager for handling communication between Activity manager
    /// and Workplace manager.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        // MirageXR app selection exit.
        public delegate void AppSelectionExitDelegate();

        public static event AppSelectionExitDelegate OnAppSelectionExit;

        public static void AppSelectionExit()
        {
            OnAppSelectionExit?.Invoke();
        }

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

        // MirageXR player reset.
        public delegate void PlayerResetDelegate();

        public static event PlayerResetDelegate OnPlayerReset;

        /// <summary>
        /// Reset MirageXR player.
        /// </summary>
        public static void PlayerReset()
        {
            OnPlayerReset?.Invoke();
        }

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

        // Workplace has been set up event.
        public delegate void WorkplaceLoadedDelegate();

        public static event WorkplaceLoadedDelegate OnWorkplaceLoaded;

        /// <summary>
        /// Signals that the data from the workplace data model has been applied to the scene.
        /// </summary>
        public static void WorkplaceLoaded()
        {
            OnWorkplaceLoaded?.Invoke();
        }

        // TODO: Is this the same as ActivityStarted? Duplicate removal?
        public delegate void StartActivityDelegate();

        public static event StartActivityDelegate OnStartActivity;

        public static void StartActivity()
        {
            OnStartActivity?.Invoke();
        }

        public delegate void ActivityStartedDelegate();

        public static event ActivityStartedDelegate OnActivityStarted;

        public static void ActivityStarted()
        {
            OnActivityStarted?.Invoke();
        }

        // Event to trigger adjusting workplace locations when a new world origin has been set.
        public delegate void CalibrateWorkplaceDelegate(Transform origin);

        public static event CalibrateWorkplaceDelegate OnCalibrateWorkplace;

        /// <summary>
        /// Triggers calibration methods.
        /// </summary>
        /// <param name="origin">New world origin.</param>
        public static void CalibrateWorkplace(Transform origin)
        {
            OnCalibrateWorkplace?.Invoke(origin);
        }

        // Workplace has been calibrated event.
        public delegate void WorkplaceCalibratedDelegate();

        public static event WorkplaceCalibratedDelegate OnWorkplaceCalibrated;

        /// <summary>
        /// Reports that calibration has been performed
        /// </summary>
        /// <param name="action">Activity file action id of the activated action.</param>
        public static void WorkplaceCalibrated()
        {
            OnWorkplaceCalibrated?.Invoke();
        }

        // Clear poi event.
        public delegate void ClearPoisDelegate();

        public static event ClearPoisDelegate OnClearPois;

        public static void ClearPois()
        {
            OnClearPois?.Invoke();
        }

        // Activate action event.
        public delegate void ActivateActionDelegate(string action);

        public static event ActivateActionDelegate OnActivateAction;

        /// <summary>
        /// Activates a new action.
        /// </summary>
        /// <param name="action">Activity file action id of the activated action.</param>
        public static void ActivateAction(string action)
        {
            OnActivateAction?.Invoke(action);
        }

        // Deactivate action event.
        public delegate void DeactivateActionDelegate(string action, bool doNotActivateNextStep = false);

        public static event DeactivateActionDelegate OnDeactivateAction;

        /// <summary>
        /// Deactivates a new action.
        /// </summary>
        /// <param name="action">Activity file action id of the deactivated action.</param>
        public static void DeactivateAction(string action, bool doNotActivateNextStep = false)
        {
            OnDeactivateAction?.Invoke(action, doNotActivateNextStep);
        }

        // Activate/Deactivate an object event.
        public delegate void ToggleObjectDelegate(ToggleObject obj, bool isActivating);

        public static event ToggleObjectDelegate OnToggleObject;

        /// <summary>
        /// Activates an object.
        /// </summary>
        /// <param name="action">Activate object of activity file action.</param>
        public static void ActivateObject(ToggleObject action)
        {
            OnToggleObject?.Invoke(action, true);
        }

        /// <summary>
        /// Deactivates an object.
        /// </summary>
        /// <param name="action">Deactivate object of activity file action.</param>
        public static void DeactivateObject(ToggleObject action)
        {
            OnToggleObject?.Invoke(action, false);
        }

        // Log debug event.
        public delegate void DebugLogDelegate(string debug);

        public static event DebugLogDelegate OnDebugLog;

        /// <summary>
        /// Adds debug message to UI debug console and normal debug log.
        /// </summary>
        /// <param name="debug">Debug message.</param>
        public static void DebugLog(string debug)
        {
            Debug.Log(debug);
            OnDebugLog?.Invoke(debug);
        }

        // Show guides event.
        public delegate void ShowGuidesDelegate();

        public static event ShowGuidesDelegate OnShowGuides;

        /// <summary>
        /// Show guidelines.
        /// </summary>
        public static void ShowGuides()
        {
            OnShowGuides?.Invoke();
        }

        // Hide guides event.
        public delegate void HideGuidesDelegate();

        public static event HideGuidesDelegate OnHideGuides;

        /// <summary>
        /// Hide guidelines.
        /// </summary>
        public static void HideGuides()
        {
            OnHideGuides?.Invoke();
        }

        public delegate void TapDelegate();

        public static event TapDelegate OnTap;

        public static void Tap()
        {
            OnTap?.Invoke();
        }

        public delegate void NextDelegate(string trigger);

        public static event NextDelegate OnNext;

        public static void Next(string trigger)
        {
            OnNext?.Invoke(trigger);
        }

        public delegate void PreviousDelegate(string trigger);

        public static event PreviousDelegate OnPrevious;

        public static void Previous(string trigger)
        {
            OnPrevious?.Invoke(trigger);
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

        public delegate void ClearAllDelegate();

        public static event ClearAllDelegate OnClearAll;

        public static void ClearAll()
        {
            OnClearAll?.Invoke();
        }

        public delegate void InitUiDelegate();

        public static event InitUiDelegate OnInitUi;

        public static void InitUi()
        {
            Debug.Log("Init UI invoked");
            OnInitUi?.Invoke();
        }

        // Player UI related events. Needed for the unified KeywordManager.
        
#region Voice

        //TODO: Replace "voice" events with a single event with an enum parameter 
        // For opening the augmentation list
        public delegate void OpenAnnotationByVoiceDelegate();

        public static event OpenAnnotationByVoiceDelegate OnOpenAnnotationByVoice;

        public static void OpenAnnotationByVoice()
        {
            OnOpenAnnotationByVoice?.Invoke();
        }


        // Player UI related events. Needed for the unified KeywordManager.

        // For deleting the active action step
        public delegate void DeleteActionByVoiceDelegate();

        public static event DeleteActionByVoiceDelegate OnDeleteActionByVoice;

        public static void DeleteActionByVoice()
        {
            OnDeleteActionByVoice?.Invoke();
        }


        // Player UI related events. Needed for the unified KeywordManager.

        // For adding a new action step
        public delegate void AddActionByVoiceDelegate();

        public static event AddActionByVoiceDelegate OnAddActionByVoice;

        public static void AddActionByVoice()
        {
            OnAddActionByVoice?.Invoke();
        }


        // Player UI related events. Needed for the unified KeywordManager.

        // For opening login page
        public delegate void LoginByVoiceDelegate();

        public static event LoginByVoiceDelegate OnLoginByVoice;

        public static void LoginByVoice()
        {
            OnLoginByVoice?.Invoke();
        }


        // Player UI related events. Needed for the unified KeywordManager.

        // For opening Moodle register page
        public delegate void RegisterByVoiceDelegate();

        public static event RegisterByVoiceDelegate OnRegisterActionByVoice;

        public static void RegisterByVoice()
        {
            OnRegisterActionByVoice?.Invoke();
        }


        // Player UI related events. Needed for the unified KeywordManager.

        // Save the active activity
        public delegate void SaveActivityByVoiceDelegate();

        public static event SaveActivityByVoiceDelegate OnSaveActivityActionByVoice;

        public static void SaveActivityByVoice()
        {
            OnSaveActivityActionByVoice?.Invoke();
        }


        // Player UI related events. Needed for the unified KeywordManager.

        // Upload the active activity
        public delegate void UploadActivityByVoiceDelegate();

        public static event UploadActivityByVoiceDelegate OnUploadActivityActionByVoice;

        public static void UploadActivityByVoice()
        {
            OnUploadActivityActionByVoice?.Invoke();
        }


        // Player UI related events. Needed for the unified KeywordManager.

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
        
#endregion

        // Do click.
        public delegate void ClickDelegate();

        public static event ClickDelegate OnClick;

        public static void Click()
        {
            OnClick?.Invoke();
        }

        // Player timestamping events.
        public delegate void ActivityLoadedStampDelegate(string deviceId, string activityId, string timestamp);

        public static event ActivityLoadedStampDelegate OnActivityLoadedStamp;

        public static void ActivityLoadedStamp(string deviceId, string activityId, string timestamp)
        {
            OnActivityLoadedStamp?.Invoke(deviceId, activityId, timestamp);

            Debug.Log($"LOADED STAMP: {deviceId}, {activityId}, {timestamp}");
        }

        public delegate void ActivityCompletedStampDelegate(string deviceId, string activityId, string timestamp);

        public static event ActivityCompletedStampDelegate OnActivityCompletedStamp;

        public static void ActivityCompletedStamp(string deviceId, string activityId, string timestamp)
        {
            OnActivityCompletedStamp?.Invoke(deviceId, activityId, timestamp);

            Debug.Log($"COMPLETED STAMP: {deviceId}, {activityId}, {timestamp}");
        }

        public delegate void StepActivatedStampDelegate(string deviceId, Action activatedAction, string timestamp);

        public static event StepActivatedStampDelegate OnStepActivatedStamp;

        public static void StepActivatedStamp(string deviceId, Action activatedAction, string timestamp)
        {
            OnStepActivatedStamp?.Invoke(deviceId, activatedAction, timestamp);

            Debug.Log($"ACTIVATED STAMP: {deviceId}, {activatedAction.id}, {timestamp}");
        }

        public delegate void StepDeactivatedStampDelegate(string deviceId, Action deactivatedAction, string timestamp);

        public static event StepDeactivatedStampDelegate OnStepDeactivatedStamp;

        public static void StepDeactivatedStamp(string deviceId, Action deactivatedAction, string timestamp)
        {
            OnStepDeactivatedStamp?.Invoke(deviceId, deactivatedAction, timestamp);

            Debug.Log("DEACTIVATED STAMP: " + deviceId + ", " + deactivatedAction.id + ", " + timestamp);
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

        public delegate void ActionDeletedDelegate(string actionId);

        public static event ActionDeletedDelegate OnActionDeleted;

        public static void NotifyActionDeleted(string actionId)
        {
            OnActionDeleted?.Invoke(actionId);
        }

        public delegate void AugmentationDeletedDelegate(ToggleObject toggleObject);

        public static event AugmentationDeletedDelegate OnAugmentationDeleted;

        public static void NotifyAugmentationDeleted(ToggleObject toggleObject)
        {
            OnAugmentationDeleted?.Invoke(toggleObject);
        }

        public delegate void ActivitySaveDelegate();

        public static event ActivitySaveDelegate OnActivitySaved;

        public static void ActivitySaved()
        {
            OnActivitySaved?.Invoke();
        }

        public delegate void ActionCreatedDelegate(Action action);

        public static event ActionCreatedDelegate OnActionCreated;

        public static void NotifyActionCreated(Action action)
        {
            OnActionCreated?.Invoke(action);
        }

        public delegate void ActionModifiedDelegate(Action action);

        public static event ActionModifiedDelegate OnActionModified;

        public static void NotifyActionModified(Action action)
        {
            OnActionModified?.Invoke(action);
        }

        public delegate void EditModeChangedDelegate(bool editModeActive);

        public static event EditModeChangedDelegate OnEditModeChanged;

        public static void NotifyEditModeChanged(bool editModeActive)
        {
            OnEditModeChanged?.Invoke(editModeActive);
        }

        public delegate void CompletedMeasuringDelegate(string measureValue, string measuringTool);

        public static event CompletedMeasuringDelegate OnCompletedMeasurement;

        public static void NotifyOnCompletedMeasuring(string measureValue, string measuringTool)
        {
            OnCompletedMeasurement?.Invoke(measureValue, measuringTool);
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

        public delegate void ActivityRenamedDelegate();
        public static event ActivityRenamedDelegate ActivityRenamed;
        public static void NotifyOnActivityRenamed()
        {
            ActivityRenamed?.Invoke();
        }

        // TODO: Give comment summary for all events.

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


        public delegate void AugmentationPoiChangedDelegate();
        public static event AugmentationPoiChangedDelegate AugmentationPoiChanged;
        public static void NotifyOnAugmentationPoiChanged()
        {
            AugmentationPoiChanged?.Invoke();
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

        public delegate void ActionStepTitleInputChangedDelegate();
        public static event ActionStepTitleInputChangedDelegate ActionStepTitleChanged;
        public static void NotifyOnActionStepTitleChanged()
        {
            ActionStepTitleChanged?.Invoke();
        }

        public delegate void ActionStepDescriptionInputChangedDelegate();
        public static event ActionStepDescriptionInputChangedDelegate ActionStepDescriptionInputChanged;
        public static void NotifyOnActionStepDescriptionInputChanged()
        {
            ActionStepDescriptionInputChanged?.Invoke();
        }
    }
}

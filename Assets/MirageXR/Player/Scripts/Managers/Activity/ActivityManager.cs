using i5.Toolkit.Core.VerboseLogging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TiltBrush;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// ActivityParser. Used for parsing Arlem activity file files
    /// and for handling activities defined in the file.
    /// </summary>
    public class ActivityManager
    {
        private const string SESSION_ID_FORMAT = "session-{0:yyyy-MM-dd_HH-mm-ss}";
        private const string WORKPLACE_ID_FORMAT = "{0}-workplace.json";

        public Activity Activity => _activity;
        public string AbsoluteURL { get; set; } // Id of the currently active action.
        public string ActiveActionId => ActiveAction != null ? ActiveAction.id : string.Empty;
        public Action ActiveAction { get; private set; }
        public string SessionId => Path.GetFileName(ActivityPath);
        public bool IsReady { get; private set; }
        public List<Action> ActionsOfTypeAction => _actionsOfTypeAction;
        public string ActivityPath  // Path for local files
        {
            get => _activityPath;
            private set
            {
                if (!Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);
                }
                _activityPath = value;
            }
        }

        private readonly ActivityRestorer _activityRestorer = new ActivityRestorer();
        private List<Action> _actionsOfTypeAction = new List<Action>();
        private string _activityPath;
        private bool _isSwitching = true; // Flag for checking if manager is in the middle of switching an activity.
        private string _activityUrl;
        private Activity _activity;
        private bool _newIdGenerated;
        private bool _mustCleanUp;
        private bool _editModeActive;

        public bool EditModeActive
        {
            get => _editModeActive;
            set
            {
                var changed = _editModeActive != value;
                _editModeActive = value;
                if (_editModeActive)
                {
                    GenerateNewId();
                }
                if (changed)
                {
                    EventManager.NotifyEditModeChanged(value);
                }
            }
        }

        public void Subscription()
        {
            EventManager.OnClearAll += Clear;
        }

        public void Unsubscribe()
        {
            EventManager.OnClearAll -= Clear;
        }

        public void OnDestroy()
        {
            if (_activity == null)
            {
                return;
            }

            // TODO: at some point we should implement a way to find out if an activity is resumable
            var isResumable = true;
            if (ActiveAction == null)
            {
                PlayerPrefs.SetString(_activity.id, "StartingAction");
                return;
            }

            var lastCompleted = ActionsOfTypeAction.IndexOf(ActiveAction) >= ActionsOfTypeAction.Count - 1 && ActiveAction.isCompleted;

            PlayerPrefs.SetString(_activity.id, !isResumable || lastCompleted ? "StartingAction" : ActiveAction.id);
            PlayerPrefs.Save();

            if (_mustCleanUp)
            {
                ActivityLocalFiles.CleanUp(_activity);
            }
        }

        private void Clear()
        {
            _activity = null;
            _activityRestorer.Clear();
        }

        /// <summary>
        /// Reset activity manager when OnPlayerReset event is triggered.
        /// </summary>
        public async Task PlayerReset()
        {
            _isSwitching = true;
            PlayerPrefs.SetString(_activity.id, "StartingAction");
            Clear();
            await LoadActivity(_activityUrl);
            EventManager.PlayerReset();
        }

        public async Task CreateNewActivity()
        {
            var activity = CreateEmptyActivity();
            _newIdGenerated = false;
            EditModeActive = false;
            await ActivateActivity(activity);
        }

        private static Activity CreateEmptyActivity()
        {
            const string cultureInfo = "en-GB";

            return new Activity
            {
                name = $"Activity ({DateTime.Now.ToString(new CultureInfo(cultureInfo))})",
                version = Application.version,
                id = string.Empty
            };
        }

        public async Task LoadActivity(string activityId)
        {
            var activity = string.IsNullOrEmpty(activityId) ? CreateEmptyActivity() : ActivityParser.Parse(activityId);
            _activityUrl = activityId;

            //Always load an existing activity in play mode
            EditModeActive = false;

            await ActivateActivity(activity);
        }

        private async Task ActivateActivity(Activity activity)
        {
            EventManager.ClearAll();
            await Task.Delay(100);

            _activity = activity;
            _actionsOfTypeAction = activity.actions.Where(t => t.type == ActionType.Action).ToList();
            EventManager.InitUi();

            EventManager.DebugLog($"Activity manager: {_activity.id} parsed.");

            ActivityPath = Path.Combine(Application.persistentDataPath, _activity.id);

            if (IsNeedToRestore(activity, out var restoreId))
            {
                _activityRestorer.RestoreActions(activity, restoreId);
            }

            await RootObject.Instance.workplaceManager.LoadWorkplace(_activity.workplace);

            await StartActivity();
            IsReady = true;
        }

        private static bool IsNeedToRestore(Activity activity, out string restoreId)
        {
            restoreId = PlayerPrefs.GetString(activity.id);
            return !restoreId.Equals("StartingAction") && !string.IsNullOrEmpty(restoreId);
        }

        /// <summary>
        /// Starts the activity when workplace file parsing is completed.
        /// </summary>
        public async Task StartActivity()
        {
            if (IsNeedToRestore(_activity, out var restoreId) && !restoreId.Equals(_activity.start))
            {
                try
                {
                    _activityRestorer.RestoreState(_activity);
                }
                catch (Exception e)
                {
                    AppLog.LogException(e);
                }
                _isSwitching = false;
                EventManager.DebugLog($"Activity manager: Starting Activity: {_activity.id}");
                EventManager.ActivityStarted();
                await ActivateAction(restoreId);
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(_activity.start))
                    {
                        _isSwitching = false;

                        await AddAction(Vector3.zero);
                        EventManager.ActivityStarted();
                        return;
                    }

                    // Activate the starting action specified in the json file.
                    foreach (var action in _activity.actions)
                    {
                        // Don't go further if the action id doesn't match.
                        if (action.id != _activity.start)
                        {
                            continue;
                        }
                        _isSwitching = false;
                        EventManager.DebugLog($"Activity manager: Starting Activity: {_activity.id}");
                        EventManager.ActivityStarted();
                        await ActivateAction(action.id);
                    }
                }
                catch (Exception e)
                {
                    Maggie.Error();
                    AppLog.LogException(e);
                }
            }
        }

        public bool IsLastAction(Action action)
        {
            var last = ActionsOfTypeAction.LastOrDefault();
            return action != null && last == action;
        }

        public void MarkCompleted(string id)
        {
            foreach (var action in _activity.actions)
            {
                if (action.id.Equals(id))
                {
                    action.isCompleted = true;
                }
            }
        }

        /// <summary>
        /// Activates an action.
        /// </summary>
        /// <param name="id">ID of the action to be activated.</param>
        private async Task ActivateAction(string id)
        {
            const string restartKey = "restart";

            try
            {
                if (id == restartKey)
                {
                    _isSwitching = true;
                    await LoadActivity(_activityUrl);
                }
                else
                {
                    var action = _activity.actions.FirstOrDefault(action => action.id == id);
                    if (action == null)
                    {
                        return;
                    }

                    await ActivateAction(action);
                }
            }
            catch (Exception e)
            {
                Maggie.Error();
                EventManager.DebugLog($"Error: Activity manager: Couldn't activate action: {id}.");
                AppLog.LogException(e);
            }
        }

        private async Task ActivateAction(Action step)
        {
            const string jsonExtension = ".json";

            ActiveAction = step;
            step.isActive = true;
            Trigger.SetupTriggers(step);

            foreach (var content in step.enter.activates)
            {
                switch (content.type)
                {
                    case ActionType.Action:
                    case ActionType.Reaction:
                        {
                            if (content.id.EndsWith(jsonExtension)) //External activity reference!
                            {
                                _isSwitching = true;
                                await LoadActivity(content.id);
                                break;
                            }

                            await ActivateAction(step.id, content);
                            break;
                        }

                    default:
                        {
                            EventManager.ActivateObject(content);
                            break;
                        }
                }

                if (_isSwitching)
                {
                    break;
                }
            }

            foreach (var deactivate in step.enter.deactivates)
            {
                switch (deactivate.type)
                {
                    case ActionType.Action:
                    case ActionType.Reaction:
                        {
                            await DeactivateAction(step.id, deactivate);
                            break;
                        }
                    default:
                        {
                            EventManager.DeactivateObject(deactivate);
                            break;
                        }
                }
            }

            var dateStamp = DateTime.UtcNow.ToUniversalTime().ToString(CultureInfo.InvariantCulture);

            EventManager.ActivateAction(step.id);
            EventManager.StepActivatedStamp(SystemInfo.deviceUniqueIdentifier, step, dateStamp);
            EventManager.DebugLog($"Activity manager: Action {step.id} activated.");
        }

        /// <summary>
        /// Deactivates an action.
        /// <param name="id">ID of the action to be deactivated.</param>
        /// </summary>
        public async Task DeactivateAction(string id, bool doNotActivateNextStep = false)
        {
            if (!_isSwitching)
            {
                //Save augmentations extra data(character, pick&place,...) for be carried over to the new action if the augmentation exists in that action
                SaveData();
                await ActivityDeactivator(id, doNotActivateNextStep);
            }
        }

        private async Task ActivityDeactivator(string id, bool doNotActivateNextStep)
        {
            const string restartKey = "restart";
            const string jsonExtension = ".json";

            // Stop any Maggie messages.
            Maggie.Stop();

            // Get rid of the triggers
            Trigger.DeleteTriggersForId(id);

            // Go through all the actions...
            foreach (var action in _activity.actions)
            {
                // Skip the non-matching actions...
                if (action.id != id)
                {
                    continue;
                }

                // Handle messages.

                ArlemMessage.ReadMessages(action);

                // Exit deactivate loop...
                foreach (var deactivate in action.exit.deactivates)
                {
                    switch (deactivate.type)
                    {
                        case ActionType.Action:
                        case ActionType.Reaction:
                            await DeactivateAction(action.id, deactivate);
                            break;

                        default:
                            EventManager.DeactivateObject(deactivate);
                            break;
                    }
                }

                await Task.Yield();

                //do not activate next step
                if (doNotActivateNextStep)
                {
                    return;
                }

                // Exit activate loop...
                foreach (var activate in action.exit.activates)
                {
                    switch (activate.type)
                    {
                        // Handle action logic related stuff here...
                        case ActionType.Action:
                        case ActionType.Reaction:
                            if (activate.id.Equals(restartKey))
                            {
                                _isSwitching = true;
                                await LoadActivity(_activityUrl);
                                break;
                            }
                            // External activity reference!
                            if (activate.id.EndsWith(jsonExtension))
                            {
                                _isSwitching = true;
                                await LoadActivity(activate.id);
                                break;
                            }

                            await ActivateAction(action.id, activate);
                            break;

                        // All the others are handled outside.
                        default:
                            EventManager.ActivateObject(activate);
                            break;
                    }

                    if (_isSwitching)
                    {
                        break;
                    }
                }

                if (_isSwitching)
                {
                    break;
                }

                // Mark as completed. Might use a bit smarter way of handling this, but meh...
                action.isCompleted = true;
                // Send timestamp.
                var timeStamp = DateTime.UtcNow.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
                EventManager.StepDeactivatedStamp(SystemInfo.deviceUniqueIdentifier, action, timeStamp);
                EventManager.DebugLog($"Activity manager: Action {id} deactivated.");
            }
        }

        /// <summary>
        /// Activate action.
        /// </summary>
        /// <param name="caller">Id of the action calling this method.</param>
        /// <param name="obj">Action toggle object.</param>
        private async Task ActivateAction(string caller, ToggleObject obj)
        {
            const string restartKey = "restart";
            const string jsonExtension = ".json";
            const string userPrefix = "user:";
            const char splitChar = ':';

            // Let's check that action really exists...
            var counter = 0;

            foreach (var actionObj in _activity.actions)
            {
                if (actionObj.id == obj.id)
                {
                    counter++;
                }
            }

            // If not, shame on you for trying!
            if (counter == 0 && obj.id != restartKey && !obj.id.EndsWith(jsonExtension))
            {
                throw new ArgumentException($"Action {obj.id} not found.");
            }

            // Check if activation is user dependent.
            if (!string.IsNullOrEmpty(obj.option) && obj.option.StartsWith(userPrefix))
            {
                // Extract the user.
                var user = obj.option.Split(splitChar)[1];

                // Activate if conditions are met.
                if (obj.id != caller && user == WorkplaceManager.GetUser())
                {
                    await ActivateAction(obj.id);
                }
            }
            // Normal activation.
            else
            {
                // Like never ending loops? I don't...
                if (obj.id != caller)
                {
                    await ActivateAction(obj.id);
                }
            }
        }

        /// <summary>
        /// Deactivate action.
        /// </summary>
        /// <param name="caller">Id of the action calling this method.</param>
        /// <param name="obj">Action toggle object.</param>
        private async Task DeactivateAction(string caller, ToggleObject obj)
        {
            // Like never ending loops? I don't...
            if (obj.id != caller)
            {
                await DeactivateAction(obj.id);
            }

            EventManager.DeactivateAction(caller);
        }

        /// <summary>
        /// Clears out the scene and activates a given action.
        /// </summary>
        /// <param name="id">Action id to be activated.</param>
        private async Task BackAction(string id)
        {
            // First clear out the scene.
            try
            {
                // Get rid of the triggers
                Trigger.DeleteTriggersForId(id);

                // Go through all the actions and try to find the active action...
                foreach (var action in _activity.actions)
                {
                    // Skip the non-matching actions...
                    if (action.id != ActiveActionId)
                    {
                        continue;
                    }

                    // Enter activate loop...
                    foreach (var activate in action.enter.activates)
                    {
                        // Do magic based on the activation object type...
                        switch (activate.type)
                        {
                            // Handle action logic related stuff here...
                            case ActionType.Action:
                            case ActionType.Reaction:
                                // We are not interested in activating anything!
                                break;

                            // All the others are handled outside.
                            default:
                                // Deactivate everything that is activated!
                                EventManager.DeactivateObject(activate);
                                break;
                        }
                    }
                }

                // Now activate the desired action on the empty slate.
                await ActivateAction(id);
            }
            catch (Exception e)
            {
                Maggie.Error();
                EventManager.DebugLog($"Error: Activity manager: Couldn't force start action: {id}.");
                AppLog.LogException(e);
                throw;
            }
        }

        public async Task ActivateActionByIndex(int index)
        {
            await DeactivateAction(ActiveAction.id, true);
            await ActivateAction(ActionsOfTypeAction[index].id);
        }

        public async Task ActivateActionByID(string id)
        {
            await DeactivateAction(ActiveAction.id, true);
            await ActivateAction(id);
        }

        public async Task ActivateNextAction()
        {
            int indexOfActivated = ActionsOfTypeAction.IndexOf(ActiveAction);
            int indexOfLast = ActionsOfTypeAction.Count - 1;
            if (indexOfActivated < indexOfLast)
            {
                if (ActiveAction != null)
                {
                    await DeactivateAction(ActiveAction.id);
                }
                else
                {
                    await ActivateAction(_activity.start);
                }
            }
        }

        public async Task ActivatePreviousAction()
        {
            int indexOfActivated = ActionsOfTypeAction.IndexOf(ActiveAction);
            if (indexOfActivated > 0)
            {
                await BackAction(ActionsOfTypeAction[indexOfActivated - 1].id);
            }
        }

        public async Task ActivateFirstAction()
        {
            int indexOfActivated = ActionsOfTypeAction.IndexOf(ActiveAction);
            if (indexOfActivated > 0)
            {
                await ActivateActionByIndex(0);
            }
        }

        public async Task ActivateLastAction()
        {
            int indexOfActivated = ActionsOfTypeAction.IndexOf(ActiveAction);
            int indexOfLastAction = Activity.actions.Count - 1;
            if (indexOfActivated < indexOfLastAction)
            {
                await ActivateActionByIndex(indexOfLastAction);
            }
        }

/*
        public void SwapActions(Action action1, Action action2)
        {
            var index1 = _activity.actions.IndexOf(action1);
            var index2 = _activity.actions.IndexOf(action2);

            if (index1 == -1 || index2 == -1)
            {
                Debug.LogError($"Could not find the {action1.id} or {action2.id} actions");
                return;
            }

            _activity.actions[index1] = action2;
            _activity.actions[index2] = action1;
        }
*/

        public async Task AddActionToBegin(Vector3 position, bool hasImageMarker = false)
        {
            var newAction = CreateAction();
            await RootObject.Instance.workplaceManager.AddPlace(newAction, position);

            _activity.start = newAction.id;
            _activity.actions.Insert(0, newAction);

            AppLog.LogDebug($"Added {newAction.id} to list of task stations");

            RegenerateActionsList();
            await ActivateNextAction();
            await ActivateAction(newAction);
            await Task.Yield();
            EventManager.NotifyActionCreated(newAction);
        }

        public async Task AddAction(Vector3 position, bool hasImageMarker = false)
        {
            // TODO: put back in the warning
            //if (taskStationList.Count == 0 && !calibrationMarkerFound)
            //{
            //    Maggie.Speak("Your workplace is not calibrated.  Recordings cannot be played back on other devices or in other workplaces.");
            //}

            // create a new arlem action representing the task station
            // (must be done before instantiating the object)
            var newAction = CreateAction();

            // create a new workplace place
            await RootObject.Instance.workplaceManager.AddPlace(newAction, position);

            int indexOfActive = -1;

            // update the exit-activate ARLEM section of previous TS to point to the new one
            if (_activity.actions.Count > 0)
            {
                // if no active action is set, use the last action as active action
                ActiveAction ??= _activity.actions[_activity.actions.Count - 1];

                indexOfActive = _activity.actions.IndexOf(ActiveAction);

                // update the exit-activate ARLEM section of previous TS to point to the new one
                if (indexOfActive >= 0)
                {
                    var exitActivateElement = new ToggleObject
                    {
                        id = newAction.id,
                        viewport = newAction.viewport,
                        type = newAction.type
                    };

                    if (_activity.actions[indexOfActive].exit.activates.Count > 0)
                    {
                        newAction.exit.activates.Add(_activity.actions[indexOfActive].exit.activates[0]);
                        _activity.actions[indexOfActive].exit.activates[0] = exitActivateElement;
                    }
                    else
                    {
                        _activity.actions[indexOfActive].exit.activates.Add(exitActivateElement);
                    }
                }
                else
                {
                    AppLog.LogError("Could not identify the active action");
                }
            }
            else
            {
                _activity.start = newAction.id;
            }

            _activity.actions.Insert(indexOfActive + 1, newAction);

            AppLog.LogDebug($"Added {newAction.id} to list of task stations");

            RegenerateActionsList();
            await ActivateNextAction();
            await ActivateAction(newAction);
            await Task.Yield();
            EventManager.NotifyActionCreated(newAction);
        }

        private Action CreateAction()
        {
            var taskStationId = Guid.NewGuid();
            var taskStationName = $"TS-{taskStationId}";

            var action = new Action
            {
                id = taskStationName,
                viewport = "actions",
                type = ActionType.Action,
                instruction = new Instruction(),
                enter = new Enter(),
                exit = new Exit(),
                triggers = new List<Trigger>(),

                device = "wekit.one",
                location = "here",
                predicate = "none",
                user = string.Empty
            };

            action.instruction.title = $"Action Step {_activity.actions.Count + 1}"; // Fudge fix: Added +1 to remove "Action Step 0". Need more information if we want to increase thisActionIndex by 1 (Might be it needs the 0)
            action.instruction.description = "Add task step description here...";

            action.AddArlemTrigger(TriggerMode.Voice);
            action.AddArlemTrigger(TriggerMode.Click);

            return action;
        }

        public void DeleteAction(string idToDelete)
        {
            int indexToDelete = _activity.actions.IndexOf(_activity.actions.FirstOrDefault(p => p.id.Equals(idToDelete)));

            if (indexToDelete < 0)
            {
                AppLog.LogError($"Could not remove {idToDelete} since the id could not be found in the list of actions");
                return;
            }

            int totalNumberOfActions = _activity.actions.Count;

            AppLog.LogInfo($"Deleting action with index {indexToDelete}...");

            if (indexToDelete == totalNumberOfActions - 1) // if deleting the last action
            {
                AppLog.LogTrace("deleting last action");

                if (totalNumberOfActions != 1)
                {
                    if (_activity.actions[indexToDelete - 1].exit.activates.Count > 0)
                    {
                        _activity.actions[indexToDelete - 1].exit.activates.Last().id = string.Empty;
                    }
                }
            }
            else if (indexToDelete == 0) // if deleting the first action
            {
                AppLog.LogTrace("deleting first action");
                // update start action with the one that is currently second (prior to deleting the action, in this case)
                if (totalNumberOfActions > 1)
                {
                    _activity.start = _activity.actions[1].id;
                }
            }
            else // we are deleting an action that is in the middle
            {
                _activity.actions[indexToDelete - 1].exit.activates.Last().id = _activity.actions[indexToDelete + 1].id;
            }

            //Create a new list of activate to avoid "Collection modified exception"
            var enterActivateTempCopy = new List<ToggleObject>();
            foreach (var item in _activity.actions[indexToDelete].enter.activates)
            {
                enterActivateTempCopy.Add(item);
            }

            var actionToDelete = _activity.actions[indexToDelete];

            var commonToggleObjects = new List<ToggleObject>();
            //find all common annotations which exist in action to delete and other actions
            foreach (var action in _activity.actions)
            {
                commonToggleObjects.AddRange(action.enter.activates.Intersect(actionToDelete.enter.activates).ToList());
            }

            foreach (var toggleObject in enterActivateTempCopy)
            {
                if (!commonToggleObjects.Contains(toggleObject))
                {
                    RootObject.Instance.augmentationManager.DeleteAugmentation(toggleObject, _activity.actions[indexToDelete]);
                }
            }

            _activity.actions.RemoveAt(indexToDelete);

            RegenerateActionsList();

            EventManager.NotifyActionDeleted(idToDelete);

            if (ActiveAction.id == idToDelete)
            {
                ActivateNextAction();
            }
        }

        private void RegenerateActionsList()
        {
            _actionsOfTypeAction.Clear();
            _actionsOfTypeAction.AddRange(_activity.actions.Where(action => action.type == ActionType.Action));
        }

        public void SaveData()
        {
            _mustCleanUp = false;
            ActivityLocalFiles.SaveData(_activity);
        }

        private void GenerateNewId(bool force = false)
        {
            if (_newIdGenerated && !force)
            {
                return;
            }

            string id;
            if (!string.IsNullOrEmpty(_activity.id) && !force)
            {
                id = _activity.id;
            }
            else
            {
                var oldId = _activity.id;
                id = string.Format(SESSION_ID_FORMAT, DateTime.UtcNow);
                if (!string.IsNullOrEmpty(oldId))
                {
                    ActivityLocalFiles.MoveData(oldId, id);
                }
                _activity.id = id;
                _activity.version = Application.version;
                ActivityPath = Path.Combine(Application.persistentDataPath, id);
                _mustCleanUp = true; // not yet saved; this is used if the user quits without saving so that we can clean up
            }

            var workplaceId = string.Format(WORKPLACE_ID_FORMAT, id);
            RootObject.Instance.workplaceManager.workplace.id = workplaceId;
            _activity.workplace = workplaceId;
            _newIdGenerated = true;
        }

        public void CloneActivity()
        {
            GenerateNewId(true);
        }
    }
}
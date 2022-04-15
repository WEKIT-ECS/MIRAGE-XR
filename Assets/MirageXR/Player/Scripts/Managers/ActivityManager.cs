using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
	/// <summary>
	/// ActivityParser. Used for parsing Arlem activity file files
	/// and for handling activities defined in the file.
	/// </summary>
	public class ActivityManager : MonoBehaviour
	{
		// Flag for checking if manager is in the middle of switching an activity.
		private bool _isSwitching = true;

		private string _activityUrl;


		// Instantiation of the activity file.
		public Activity Activity;


		public string AbsoluteURL
        {
			get; set;
        }

		// Id of the currently active action.
		public string ActiveActionId
		{
			get
			{
				return ActiveAction != null ? ActiveAction.id : string.Empty;
			}
		}

		public Action ActiveAction
		{
			get; private set;
		}

		// Path for local files
		public string Path { get; private set; }

		public string SessionId => System.IO.Path.GetFileName(Path);

		public bool IsReady;

		[SerializeField]
		private List<string> RestoreActions = new List<string>();

		[SerializeField]
		private List<string> RestoreReactions = new List<string>();

		[SerializeField]
		private List<ToggleObject> RestoreObjects = new List<ToggleObject>();


		public static ActivityManager Instance { get; private set; }

		public List<Action> ActionsOfTypeAction { get; private set; }

		private bool editModeActive;

		public bool EditModeActive
		{
			get => editModeActive;
			set
			{
				bool changed = editModeActive != value;
				editModeActive = value;
                if (editModeActive)
                {
                    GenerateNewId();
                }
                if (changed)
				{
					EventManager.NotifyEditModeChanged(value);
				}

			}
		}


		private bool newIdGenerated = false;
		private bool mustCleanUp = false;

		private void Awake()
		{
			Instance = this;
		}

		private void OnEnable()
		{
			// Register to event manager events.
			EventManager.OnPlayerReset += PlayerReset;
			EventManager.OnParseActivity += ParseActivity;
			EventManager.OnStartActivity += StartActivity;
			EventManager.OnDeactivateAction += DeactivateAction;
			EventManager.OnGoBack += BackAction;
			EventManager.OnClearAll += Clear;
			EventManager.OnMarkCompleted += OnMarkCompleted;
			EventManager.OnWorkplaceParsed += GetReady;
		}

		private void OnDisable()
		{
			// Unregister from event manager events.
			EventManager.OnPlayerReset -= PlayerReset;
			EventManager.OnParseActivity -= ParseActivity;
			EventManager.OnStartActivity -= StartActivity;
			EventManager.OnDeactivateAction -= DeactivateAction;
			EventManager.OnGoBack -= BackAction;
			EventManager.OnClearAll -= Clear;
			EventManager.OnMarkCompleted -= OnMarkCompleted;
			EventManager.OnWorkplaceParsed -= GetReady;
		}

		private void OnDestroy()
		{
			if (Activity == null)
			{
				return;
			}

			// TODO: at some point we should implement a way to find out if an activity is resumable
			bool isResumable = true;

			if (ActiveAction == null)
			{
				PlayerPrefs.SetString(Activity.id, "StartingAction");
				return;
			}

			bool lastCompleted = ActionsOfTypeAction.IndexOf(ActiveAction) >= ActionsOfTypeAction.Count - 1 && ActiveAction.isCompleted;

			if (!isResumable || lastCompleted)
			{
				PlayerPrefs.SetString(Activity.id, "StartingAction");
			}
			else
			{
				PlayerPrefs.SetString(Activity.id, ActiveAction.id);
			}

			PlayerPrefs.Save();

			if (mustCleanUp)
			{
				CleanUp();
			}
		}



		private void Clear()
		{
			Activity = null;
			RestoreActions.Clear();
			RestoreReactions.Clear();
		}

		public void DoReplay()
		{
			Maggie.Ok();
			EventManager.ClearPois();
			Invoke(nameof(PlayerReset), 5f);
		}

		/// <summary>
		/// Reset activity manager when OnPlayerReset event is triggered.
		/// </summary>
		public void PlayerReset()
		{
			_isSwitching = true;
			PlayerPrefs.SetString(Activity.id, "StartingAction");
			RestoreActions.Clear();
			RestoreReactions.Clear();
			EventManager.ParseActivity(_activityUrl);
		}


		private void GetReady()
		{
			IsReady = true;
			StartActivity();
		}

		// Called from the event manager.
		public void ParseActivity(string activity)
		{
			StartCoroutine(ActivityParser(activity));
		}

		/// <summary>
		/// Parses the activity file JSON. Called from the event manager.
		/// </summary>
		/// <param name="activity">ID of the activity file JSON file.</param>
		private IEnumerator ActivityParser(string activity)
		{
			Maggie.Stop();

			IsReady = false;

			// Clear all to enable clean slate.
			EventManager.ClearAll();

			// Wait for clearing to complete.
			yield return null;

			EventManager.DebugLog("Parsing: " + activity);

			var errorCount = 0;

			// load a new activity
			if (string.IsNullOrEmpty(activity))
			{
				Activity = new Activity();
				newIdGenerated = false;
				EditModeActive = false;
				Activity.name = "New Activity";
				Activity.version = Application.version;
			}
			// Load from resources.
			else if (activity.StartsWith("resources://"))
			{
				var asset = Resources.Load(activity.Replace("resources://", "")) as TextAsset;

				// Create workplace file object from json
				try
				{
					// For loading from resources
					Activity = JsonUtility.FromJson<Activity>(asset.text);
				}
				catch (System.Exception e)
				{
					EventManager.DebugLog("Error: Activity manager: Parsing: Couldn't parse the Activity json: " + e.ToString());
					errorCount++;
				}
			}
			// For loading from application path.
			else
			{
				if (activity.StartsWith("http"))
				{
					var fullname = activity.Split('/');
					activity = fullname[fullname.Length - 1];
				}

				if (!activity.EndsWith(".json"))
					activity += ".json";

				var url = File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, activity));

				try
				{
					Activity = JsonUtility.FromJson<Activity>(url);
				}
				catch (Exception e)
				{
					EventManager.DebugLog($"Error: Activity manager: Parsing: Couldn't parse the Activity json: {e}");
					errorCount++;
				}
			}

			if (errorCount.Equals(0))
			{
				try
				{
					ActionsOfTypeAction = new List<Action>();
					foreach (var action in Activity.actions)
					{
						if (action.type.Equals("action"))
						{
							ActionsOfTypeAction.Add(action);
						}
					}
				}
				catch (Exception e)
				{
					errorCount++;
					Debug.Log(e);
				}

				// Wait for actions list to complete.
				yield return new WaitForSeconds(1f);

				// Init ui components.
				EventManager.InitUi();
			}

			// If all good...
			if (errorCount.Equals(0))
			{
				// We made it!
				EventManager.DebugLog($"Activity manager: {Activity.id} parsed.");
				Path = System.IO.Path.Combine(Application.persistentDataPath, Activity.id);

				// For reset functionality.
				_activityUrl = activity;

				UiManager.Instance.WelcomeMessage = Activity.message;

				var restoreState = PlayerPrefs.GetString(Activity.id);

				if (!restoreState.Equals("StartingAction") && !string.IsNullOrEmpty(restoreState))
				{
					if (!string.IsNullOrEmpty(Activity.start))
						RestoreActions.Add(Activity.start);

					foreach (var action in Activity.actions)
					{
						// Continue only until restore step is reached.
						if (action.id.Equals(restoreState))
							break;

						foreach (var activate in action.enter.activates)
						{
							switch (activate.type)
							{
								case "action":
									if (!activate.id.Equals(restoreState))
										RestoreActions.Add(activate.id);
									break;
								case "reaction":
									break;
							}
						}

						foreach (var deactivate in action.enter.deactivates)
						{
							switch (deactivate.type)
							{
								case "action":
								case "reaction":
									break;
							}
						}

						foreach (var deactivate in action.exit.deactivates)
						{
							switch (deactivate.type)
							{
								case "action":
								case "reaction":
									break;
							}
						}

						foreach (var activate in action.exit.activates)
						{
							switch (activate.type)
							{
								case "action":
									if (!activate.id.Equals(restoreState))
										RestoreActions.Add(activate.id);
									break;
								case "reaction":
									break;
							}
						}
					}

					foreach (var restoreAction in RestoreActions)
					{
						foreach (var action in Activity.actions)
						{
							if (action.id.Equals(restoreAction))
							{
								action.isCompleted = true;
							}
						}
					}
				}

				EventManager.ParseWorkplace(Activity.workplace);
			}

			else
			{
				Maggie.Error();
			}
		}

		/// <summary>
		/// Starts the activity when workplace file parsing is completed.
		/// </summary>
		public async void StartActivity()
		{
			var restoreState = PlayerPrefs.GetString(Activity.id);

			Debug.Log($"Activity manager restore state:{restoreState}. IsNullOrEmpty? {string.IsNullOrEmpty(restoreState)}");

			if (!string.IsNullOrEmpty(restoreState) && !restoreState.Equals(Activity.start) && !restoreState.Equals("StartingAction"))
			{
				foreach (var restoreAction in RestoreActions)
				{
					foreach (var action in Activity.actions)
					{
						if (action.id.Equals(restoreAction))
						{
							foreach (var activate in action.enter.activates)
							{
								switch (activate.type)
								{
									case "action":
										break;
									case "reaction":
										RestoreReactions.Add(activate.id);
										break;
									default:
										RestoreObjects.Add(activate);
										break;
								}
							}

							foreach (var deactivate in action.enter.deactivates)
							{
								switch (deactivate.type)
								{
									case "action":
										break;
									case "reaction":
										if (RestoreReactions.Contains(deactivate.id))
											RestoreReactions.Remove(deactivate.id);
										break;
									default:
										RemoveObject(deactivate);
										break;
								}
							}

							foreach (var deactivate in action.exit.deactivates)
							{
								switch (deactivate.type)
								{
									case "action":
										break;
									case "reaction":
										if (RestoreReactions.Contains(deactivate.id))
											RestoreReactions.Remove(deactivate.id);
										break;
									default:
										RemoveObject(deactivate);
										break;
								}
							}

							foreach (var activate in action.exit.activates)
							{
								switch (activate.type)
								{
									case "action":
										break;
									case "reaction":
										RestoreReactions.Add(activate.id);
										break;
									default:
										RestoreObjects.Add(activate);
										break;
								}
							}
						}
					}
				}

				foreach (var restoreObject in RestoreObjects)
				{
					EventManager.ActivateObject(restoreObject);
				}

				foreach (var reaction in RestoreReactions)
				{
					foreach (var action in Activity.actions)
					{
						if (!action.type.Equals("reaction"))
							continue;

						if (action.id.Equals(reaction))
						{
							foreach (var trigger in action.triggers)
							{
								// Every trigger need to have a mode.
								if (string.IsNullOrEmpty(trigger.mode))
									throw new ArgumentException("Trigger mode not set.");

								switch (trigger.mode)
								{
									case "click":
									case "character":
									case "audio":
									case "video":
									case "voice":
										// Handled inside the ActivityCard.cs
										break;

									case "sensor":
										var smartObj = Utilities.CreateObject(action.id + "_smartTrigger_" + trigger.id,
											"Triggers");

										if (smartObj == null)
											throw new MissingComponentException("Couldn't create the smart trigger object.");

										var smartBehaviour = smartObj.AddComponent<SmartTrigger>();

										// Try to create the trigger.
										if (!smartBehaviour.CreateTrigger(action.id, trigger.id, trigger.data,
											trigger.option, trigger.value, trigger.duration))
										{
											Destroy(smartObj);
											throw new MissingComponentException("Couldn't create the smart trigger.");
										}

										// Activate trigger
										smartBehaviour.Activate();

										break;

									default:
										throw new ArgumentException("Unknown trigger mode: " + trigger.mode);
								}
							}
						}
					}
				}
				_isSwitching = false;
				EventManager.DebugLog("Activity manager: Starting Activity: " + Activity.id);
				EventManager.ActivityStarted();
				ActivateAction(restoreState);
			}
			else
			{
				try
                {
	                if (string.IsNullOrEmpty(Activity.start))
	                {
		                _isSwitching = false;
		                EventManager.ActivityStarted();
		                await AddAction(Vector3.zero);
		                return;
	                }

	                // Activate the starting action specified in the json file.
	                foreach (var action in Activity.actions)
	                {
		                // Don't go further if the action id doesn't match.
		                if (action.id != Activity.start)
		                {
			                continue;
		                }
		                _isSwitching = false;
		                EventManager.DebugLog("Activity manager: Starting Activity: " + Activity.id);
		                EventManager.ActivityStarted();
		                ActivateAction(action.id);
	                }
                }
                catch (Exception e)
                {
                    Maggie.Error();
                    Debug.Log(e);
                    throw;
                }
            }
		}

		private void RemoveObject(ToggleObject obj)
		{
			for (int i = 0; i < RestoreObjects.Count; i++)
			{
				if (RestoreObjects[i].id.Equals(obj.id))
				{
					if (RestoreObjects[i].poi.Equals(obj.poi))
					{
						if (RestoreObjects[i].position.Equals(obj.position) && RestoreObjects[i].rotation.Equals(obj.rotation))
						{
							if (RestoreObjects[i].predicate.Equals(obj.predicate))
							{
								if (!string.IsNullOrEmpty(RestoreObjects[i].url))
								{
									if (RestoreObjects[i].url.Equals(obj.url))
									{
										RestoreObjects.Remove(RestoreObjects[i]);
									}
								}
								else
								{
									RestoreObjects.Remove(RestoreObjects[i]);
								}
							}
						}
					}
				}
			}
		}

		public bool IsLastAction(Action action)
		{
			var last = ActionsOfTypeAction.LastOrDefault();
			return action != null && last == action;
		}
		
		public void StartActivityTouch()
		{
			EventManager.Click();
			StartActivity();
		}

		private void OnMarkCompleted(string id)
		{
			foreach (var action in Activity.actions)
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
		private void ActivateAction(string id)
		{
			try
			{
				if (id == "restart")
				{
					_isSwitching = true;
					EventManager.ParseActivity(_activityUrl);
				}
				else
				{
					// Go through all the actions...
					foreach (var action in Activity.actions)
					{
						// Skip the non-matching ones.
						if (action.id != id)
							continue;

						ActiveAction = action;

						// Activate new action...
						action.isActive = true;

						// Counter for messages. Used for checking if the starting action contains a message to be played back...
						var messageCount = 0;

						// Handle messages.
						foreach (var message in action.enter.messages)
						{
							if (!string.IsNullOrEmpty(message.target) && !string.IsNullOrEmpty(message.text))
							{
								var msg = new ArlemMessage(message.target, message.text);
								messageCount++;
							}
						}

						//// Check if activated action is the starting action and if there are no messages...
						//if (action.id.Equals(Activity.start) && messageCount.Equals(0))
						//{
						//	// ...and if so, play random start message
						//	Maggie.ActivityReady();
						//}

						// Handle triggers.
						foreach (var trigger in action.triggers)
						{
							// Every trigger need to have a mode.
							if (string.IsNullOrEmpty(trigger.mode))
								throw new ArgumentException("Trigger mode not set.");

							switch (trigger.mode)
							{
								case "detect":
								case "click":
								case "character":
								case "audio":
								case "video":
								case "voice":
									// Handled inside the ActivityCard.cs
									break;

								case "sensor":
									var smartObj = Utilities.CreateObject($"{action.id}_smartTrigger_{trigger.id}",
										"Triggers");

									if (smartObj == null)
										throw new MissingComponentException("Couldn't create the smart trigger object.");

									var smartBehaviour = smartObj.AddComponent<SmartTrigger>();

									// Try to create the trigger.
									if (
										!smartBehaviour.CreateTrigger(action.id, trigger.id, trigger.data,
											trigger.option, trigger.value, trigger.duration))
									{
										Destroy(smartObj);
										throw new MissingComponentException("Couldn't create the smart trigger.");
									}

									// Activate trigger
									smartBehaviour.Activate();

									break;

								default:
									throw new ArgumentException("Unknown trigger mode: " + trigger.mode);
							}
						}

						// Enter activate loop...
						foreach (var activate in action.enter.activates)
						{
							// Do magic based on the activation object type...
							switch (activate.type)
							{
								// Handle action logic related stuff here...
								case "action":
								case "reaction":

									// External activity reference!
									if (activate.id.EndsWith(".json"))
									{
										_isSwitching = true;
										EventManager.ParseActivity(activate.id);
										break;
									}

									ActivateAction(action.id, activate);
									break;

								// All the others are handled outside.
								default:
									EventManager.ActivateObject(activate);
									break;
							}
							// Break from activation loop if switching.
							if (_isSwitching)
								break;
						}

						// Break from main loop if switching.
						if (_isSwitching)
							break;

						// Enter deactivate loop...
						foreach (var deactivate in action.enter.deactivates)
						{
							// Do magic based on the deactivation object type...
							switch (deactivate.type)
							{
								// Handle action logic related stuff here...
								case "action":
								case "reaction":
									DeactivateAction(action.id, deactivate);
									break;

								// All the others are handled outside.
								default:
									EventManager.DeactivateObject(deactivate);
									break;
							}
						}

						var idToActive = (id != "" && id != "restart") ? id : ActiveActionId;
						EventManager.ActivateAction(idToActive);

						EventManager.StepActivatedStamp(SystemInfo.deviceUniqueIdentifier, action, DateTime.UtcNow.ToUniversalTime().ToString());
						EventManager.DebugLog($"Activity manager: Action {id} activated.");
					}
				}
			}
			catch (Exception e)
			{
				Maggie.Error();
				EventManager.DebugLog($"Error: Activity manager: Couldn't activate action: {id}.");
				Debug.Log(e);
				throw;
			}
		}

		/// <summary>
		/// Deactivates an action.
		/// <param name="id">ID of the action to be deactivated.</param>
		/// </summary>
		private void DeactivateAction(string id, bool doNotActivateNextStep = false)
		{
			if (!_isSwitching)
				StartCoroutine(ActivityDeactivator(id, doNotActivateNextStep));
		}

		private IEnumerator ActivityDeactivator(string id, bool doNotActivateNextStep)
		{
			// Stop any Maggie messages.
			Maggie.Stop();

			// Get rid of the triggers
			foreach (Transform trigger in GameObject.Find("Triggers").transform)
			{
				if (trigger.gameObject.name.StartsWith(id))
				{
					Destroy(trigger.gameObject);
				}
			}

			// Go through all the actions...
			for (int i = 0; i < Activity.actions.Count; i++)
			{
				var action = Activity.actions[i];

				// Skip the non-matching actions...
				if (action.id != id)
					continue;


				// Handle messages.
				foreach (var message in action.exit.messages)
				{
					if (string.IsNullOrEmpty(message.target))
						continue;

					if (string.IsNullOrEmpty(message.text))
						continue;

					var msg = new ArlemMessage(message.target, message.text);
				}

				// Exit deactivate loop...
				foreach (var deactivate in action.exit.deactivates)
				{
					// Do magic based on the deactivation object type...
					switch (deactivate.type)
					{
						// Handle action logic related stuff here...
						case "action":
						case "reaction":
							DeactivateAction(action.id, deactivate);
							break;

						// All the others are handled outside.
						default:
							EventManager.DeactivateObject(deactivate);
							break;
					}
				}

				// Wait a bit just to make sure everything is properly deactivated before doing anything else...
				yield return new WaitForSeconds(0.5f);

				//do not activate next step
				if (doNotActivateNextStep)
					yield break; 

				// Exit activate loop...
				foreach (var activate in action.exit.activates)
				{
					// Do magic based on the activation object type...
					switch (activate.type)
					{
						// Handle action logic related stuff here...
						case "action":
						case "reaction":
							if (activate.id.Equals("restart"))
							{
								_isSwitching = true;
								EventManager.ParseActivity(_activityUrl);
								break;
							}
							// External activity reference!
							if (activate.id.EndsWith(".json"))
							{
								_isSwitching = true;
								EventManager.ParseActivity(activate.id);
								break;
							}

							ActivateAction(action.id, activate);
							break;

						// All the others are handled outside.
						default:
							EventManager.ActivateObject(activate);
							break;
					}

					if (_isSwitching)
						break;
				}

				if (_isSwitching)
					break;

				// Mark as completed. Might use a bit smarter way of handling this, but meh...
				action.isCompleted = true;
				// Send timestamp.
				EventManager.StepDeactivatedStamp(SystemInfo.deviceUniqueIdentifier, action, DateTime.UtcNow.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
				EventManager.DebugLog($"Activity manager: Action {id} deactivated.");
			}			
		}

		/// <summary>
		/// Activate action.
		/// </summary>
		/// <param name="caller">Id of the action calling this method.</param>
		/// <param name="obj">Action toggle object.</param>
		private void ActivateAction(string caller, ToggleObject obj)
		{
			// Let's check that action really exists...
			var counter = 0;

			foreach (var actionObj in Activity.actions)
			{
				if (actionObj.id == obj.id)
					counter++;
			}

			// If not, shame on you for trying!
			if (counter == 0 && obj.id != "restart" && !obj.id.EndsWith(".json"))
				throw new ArgumentException($"Action {obj.id} not found.");

			// Check if activation is user dependent.
			if (obj.option.StartsWith("user:"))
			{
				// Extract the user.
				var user = obj.option.Split(':')[1];

				// Activate if conditions are met.
				if (obj.id != caller && user == WorkplaceManager.GetUser())
					ActivateAction(obj.id);
			}

			// Normal activation.
			else
			{
				// Like neverending loops? I don't...
				if (obj.id != caller)
					ActivateAction(obj.id);
			}
		}

		/// <summary>
		/// Deactivate action.
		/// </summary>
		/// <param name="caller">Id of the action calling this method.</param>
		/// <param name="obj">Action toggle object.</param>
		private void DeactivateAction(string caller, ToggleObject obj)
		{
			// Like never ending loops? I don't...
			if (obj.id != caller)
				DeactivateAction(obj.id);
		}

		/// <summary>
		/// Clears out the scene and activates a given action.
		/// </summary>
		/// <param name="id">Action id to be activated.</param>
		private void BackAction(string id)
		{
            // First clear out the scene.
            try
            {
                // Get rid of the triggers
                foreach (Transform trigger in GameObject.Find("Triggers").transform)
				{
					if (trigger.gameObject.name.StartsWith(id))
						Destroy(trigger.gameObject);
				}

				// Go through all the actions and try to find the active action...
				foreach (var action in Activity.actions)
				{
					// Skip the non-matching actions...
					if (action.id != ActiveActionId)
						continue;

					// Enter activate loop...
					foreach (var activate in action.enter.activates)
					{
						// Do magic based on the activation object type...
						switch (activate.type)
						{
							// Handle action logic related stuff here...
							case "action":
							case "reaction":
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
				ActivateAction(id);
        }
			catch (Exception e)
			{
				Maggie.Error();
				EventManager.DebugLog("Error: Activity manager: Couldn't force start action: " + id + ".");
				Debug.Log(e);
				throw;
			}
}


		public void ActivateActionByIndex(int index)
        {
			DeactivateAction(ActiveAction.id, true);
			ActivateAction(ActionsOfTypeAction[index].id);
		}


		public void ActivateNextAction()
		{
			if (ActiveAction != null)
			{
				DeactivateAction(ActiveAction.id);
			}
			else
			{
				ActivateAction(Activity.start);
			}
		}


		public void ActivatePreviousAction()
		{
			int indexOfActivated = ActionsOfTypeAction.IndexOf(ActiveAction);
			if (indexOfActivated > 0)
			{
				BackAction(ActionsOfTypeAction[indexOfActivated - 1].id);
			}
		}

		public async Task<Action> AddAction(Vector3 position, bool hasImageMarker = false)
		{
			// TODO: put back in the warning
			//if (taskStationList.Count == 0 && !calibrationMarkerFound)
			//{
			//    Maggie.Speak(
			//        "Your workplace is not calibrated.  Recordings cannot be played back on other devices or in other workplaces.");
			//}

			// create a new arlem action representing the task station
			// (must be done before instantiating the object)
			Action newAction = InitializeAction();

			// create a new workplace place
			WorkplaceManager.Instance.AddPlace(newAction, position);

			int indexOfActive = -1;

			// update the exit-activate ARLEM section of previous TS to point to the new one
			if (Activity.actions.Count > 0)
			{
				// if no active action is set, use the last action as active action
				if (ActiveAction == null)
				{
					ActiveAction = Activity.actions[Activity.actions.Count - 1];
				}

				indexOfActive = Activity.actions.IndexOf(ActiveAction);

				// update the exit-activate ARLEM section of previous TS to point to the new one
				if (indexOfActive >= 0)
				{
                    var exitActivateElement = new Activate
                    {
                        id = newAction.id,
                        viewport = newAction.viewport,
                        type = newAction.type
                    };

                    if (Activity.actions[indexOfActive].exit.activates.Count > 0)
					{
						newAction.exit.activates.Add(Activity.actions[indexOfActive].exit.activates[0]);
						Activity.actions[indexOfActive].exit.activates[0] = exitActivateElement;
					}
					else
					{
						Activity.actions[indexOfActive].exit.activates.Add(exitActivateElement);
					}
				}
				else
				{
					Debug.LogError("Could not identify the active action", this);
				}

				//Save augmentations extra data(character, pick&place,...) for be carried over to the new action if the augmentation exists in that action
				SaveData();
			}
			else
			{
				Activity.start = newAction.id;
			}

			Activity.actions.Insert(indexOfActive + 1, newAction);

			Debug.Log($"Added {newAction.id} to list of task stations");

			RegenerateActionsList();

			ActivateNextAction();

			EventManager.NotifyActionCreated(newAction);
			ActivateAction(newAction.id);

			await Task.Delay(1);

			return newAction;
		}

		private Action InitializeAction()
		{
			// name and link gameobject to arlem data model
			var taskStationId = Guid.NewGuid();
			var taskStationName = $"TS-{taskStationId}";

            // specify action values
            var action = new Action
            {
                id = taskStationName,
                viewport = "actions",
                type = "action",
                instruction = new Instruction(),
                enter = new Enter(),
                exit = new Exit(),
                triggers = new List<Trigger>(),

                device = "wekit.one",
                location = "here",
                predicate = "none",
                user = ""
            };

            action.instruction.title = $"Action Step {(Activity.actions.Count + 1)}"; // Fudge fix: Added +1 to remove "Action Step 0". Need more information if we want to increase thisActionIndex by 1 (Might be it needs the 0)
			action.instruction.description = "Add task step description here...";

			action.AddArlemTrigger("voice");
			action.AddArlemTrigger("click");

			return action;
		}

		public void DeleteAction(string idToDelete)
		{
			int indexToDelete = Activity.actions.IndexOf(Activity.actions.FirstOrDefault(p => p.id.Equals(idToDelete)));

			if (indexToDelete < 0)
			{
				Debug.LogError($"Could not remove {idToDelete} since the id could not be found in the list of actions");
				return;
			}

			int totalNumberOfActions = Activity.actions.Count;

			Debug.Log($"Planning to delete action with index {indexToDelete}");

			if (indexToDelete == totalNumberOfActions - 1)          // if deleting the last action
			{
				Debug.Log("deleting last action");

				if (totalNumberOfActions != 1)
				{
					Activity.actions[indexToDelete - 1].exit.activates.Last().id = "";
				}
			}
			else if (indexToDelete == 0)                // if deleting the first action
			{
				Debug.Log("deleting first action");
				// update start action with the one that is currently second (prior to deleting the action, in this case)
				if (totalNumberOfActions > 1)
				{
					Activity.start = Activity.actions[1].id;
				}
			}

			else                                    // we are deleting an action that is in the middle
			{
				Activity.actions[indexToDelete - 1].exit.activates.Last().id = Activity.actions[indexToDelete + 1].id;
			}

			// Confirm and delete
			DialogWindow.Instance.Show("Warning!",			//TODO: I don't think we should call ui functionality from core functions.
			"Are you sure you want to delete this step?",
			new DialogButtonContent("Yes", delegate { DeleteConfirmed(indexToDelete, idToDelete); }),
			new DialogButtonContent("No"));
		}


		private void DeleteConfirmed(int indexToDelete, string idToDelete)
        {
			//Create a new list of activate to avoid "Collection modified exception"
			List<ToggleObject> enter_activate_temp_copy = new List<ToggleObject>();
			foreach (var item in Activity.actions[indexToDelete].enter.activates)
				enter_activate_temp_copy.Add(item);

			var actionToDelete = Activity.actions[indexToDelete];

			List<ToggleObject> commonToggleObjects = new List<ToggleObject>();
			//find all common annotations which exist in action to delete and other actions
			foreach (var action in Activity.actions)
				commonToggleObjects.AddRange(action.enter.activates.Intersect(actionToDelete.enter.activates).ToList());

			//only delete the annotaitons which don't exist in other steps
			foreach (var toggleObject in enter_activate_temp_copy)
				if (!commonToggleObjects.Contains(toggleObject))
					DeleteAnnotation(toggleObject, Activity.actions[indexToDelete]);

			Activity.actions.RemoveAt(indexToDelete);

			RegenerateActionsList();
			ActiveAction = null;

			EventManager.NotifyActionDeleted(idToDelete);
			EventManager.ActivateAction(string.Empty);
		}


		public ToggleObject AddAnnotation(Action action, Vector3 position)
		{
			var annotation = new ToggleObject()
			{
				id = action.id,
				poi = $"AN-{Guid.NewGuid()}",
				type = "tangible",
				scale = 1
			};

			//the current action index
			var currentActionIndex = ActionsOfTypeAction.IndexOf(ActionsOfTypeAction.Find(a => a.id == ActiveActionId));
			AddAllAnnotationsBetweenSteps(currentActionIndex, currentActionIndex, annotation, position);

			return annotation;
		}


		public void AddAllAnnotationsBetweenSteps(int startIndex, int endIndex, ToggleObject annotation, Vector3 position)
        {
			var actionList = ActionsOfTypeAction;
			//remove the annotation from out of the selected steps range
			if(startIndex != 0)
            {
				for (int i = 0; i < startIndex; i++)
				{
					if (actionList[i].enter.activates.Find(p => p.poi == annotation.poi) == null) continue;

					foreach (var anno in actionList[i].enter.activates.FindAll(p => p.poi == annotation.poi))
						DeleteAnnotationFromStep(actionList[i], anno);

					if (actionList[i] == ActiveAction)
						EventManager.DeactivateObject(annotation);
				}
			}

			if(endIndex != actionList.Count - 1)
            {
				for (int i = endIndex + 1; i < actionList.Count; i++)
				{
					if (actionList[i].enter.activates.Find(p => p.poi == annotation.poi) == null) continue;

					foreach (var anno in actionList[i].enter.activates.FindAll(p => p.poi == annotation.poi))
						DeleteAnnotationFromStep(actionList[i], anno);

					if (actionList[i] == ActiveAction)
						EventManager.DeactivateObject(annotation);
				}
			}

			//add the annotation to the selected steps range if already not exist
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (actionList[i].enter.activates.Find(p => p.poi == annotation.poi) == null)
					actionList[i].enter.activates.Add(annotation);

				if (actionList[i].exit.deactivates.Find(p => p.poi == annotation.poi) == null)
					// also create an exit object (w/out activate options)
					actionList[i].exit.deactivates.Add(annotation);
			}

			//the objects of the annotation will be created only for the original step not all steps.
			WorkplaceManager.Instance.AddAnnotation(ActiveAction, annotation, position == Vector3.zero ? Vector3.zero : position); //TODO: I don't see the logic in this expression 'position == Vector3.zero ? Vector3.zero : position'
		}


		void DeleteAnnotationFromStep(Action action, ToggleObject annotation)
        {
			action.enter.activates.Remove(annotation);
			action.exit.deactivates.Remove(annotation);
		}


		public void DeleteAnnotation(ToggleObject annotation, Action step = null)
		{
			var poi = annotation.poi;

			//close all editor before deleting an annotation, some pois like audio throw exception if you delete the file but the editor is open
			FindObjectOfType<ActionEditor>().DisableAllPoiEditors();

			//delete the annotation form all steps which include the annotation

			//annotation does not exist in other step, then delete it and it's file from anywhere
			if(step == null)
            {
				foreach (var actionObj in ActionsOfTypeAction)
				{

					//remove this from triggers
					var trigger = actionObj.triggers.Find(t => t.id == poi);
					actionObj.triggers.Remove(trigger);

					//remove this from activates
					foreach (var anno in actionObj.enter.activates.FindAll(p => p.poi == poi))
					{
						DeleteAnnotationFromStep(actionObj, anno);
						if (step == null)
						{
							DeleteAnnotationFile(anno);
							WorkplaceManager.Instance.DeleteAnnotation(actionObj, anno);
						}

					}
					EventManager.NotifyActionModified(actionObj);

					//save data(annotations) after deleting additional files like character or models data
					SaveData();

					if (step == null)
					{
						//if annotaion is a character, remove the data from the session folder
						DeleteCharacterData(poi, annotation.option);

						// if augmentation is a model, remove the data from the session folder
						DeleteModelData(annotation);

						// if augmentation is a pick&place , remove the data from the session folder
						DeletePickandPlaceData(poi);
					}
				}
            }
            else
            {
				foreach (var anno in step.enter.activates.FindAll(p => p.poi == poi))
				{
					DeleteAnnotationFromStep(step, anno);
					if (step == null)
					{
						DeleteAnnotationFile(anno);
						WorkplaceManager.Instance.DeleteAnnotation(step, anno);
					}

				}
				EventManager.NotifyActionModified(step);

				//save data(annotations) after deleting additional files like character or models data
				SaveData();
			}

		}


		private void DeleteCharacterData(string poi, string chararcterName)
        {
			//delete character json
			var jsonPath = System.IO.Path.Combine(Path, $"characterinfo/{poi}.json");
			if (GameObject.Find(poi))
			{
				if (File.Exists(jsonPath))
				{
					//delete the json
					File.Delete(jsonPath);
				}

				//delete AssetBundle
				var characterDataFolder = System.IO.Path.Combine(Path, $"characterinfo"); 
				var assetBundlePath = $"{characterDataFolder}/{chararcterName}";
				if (File.Exists(assetBundlePath))
				{
					File.Delete(assetBundlePath + ".manifest");
					File.Delete(assetBundlePath);
				}

				//if only character dialog is deleted
				var audioPlayer = GameObject.Find(poi).GetComponentInChildren<AudioPlayer>();
                if (audioPlayer)
                {
					var dialogpath = audioPlayer.audioName.Replace("http:/", Path);
					var audioFileCustomPrefix = "http://characterinfo/" + ActiveActionId + "_";
					if (File.Exists(dialogpath) && GameObject.Find(audioPlayer.audioName.Replace(audioFileCustomPrefix, "").Replace(".wav", "")))
					{
						//reset the dialog recorder for recording again if needed
						var character = GameObject.Find(audioPlayer.audioName.Replace(audioFileCustomPrefix, "").Replace(".wav", "")).GetComponentInChildren<CharacterController>();
						character.DialogRecorder.ResetDialogRecorder();

						//delete the old dialog
						File.Delete(dialogpath);
					}
				}

			}
		}

		private void DeleteModelData(ToggleObject augmentation)
        {
			// avoid modifying folders if not a model augmentation
			if (!augmentation.predicate.StartsWith("3d"))
            {
				return;
            }

			// check for existing model folder and delete if necessary
			string folderName = augmentation.option;
			string modelFolderPath = System.IO.Path.Combine(Path, folderName);

			if (Directory.Exists(modelFolderPath))
            {
				Debug.Log("found model folder (" + modelFolderPath + "). Deleting...");
				DeleteAllFilesInDirectory(modelFolderPath);
				Directory.Delete(modelFolderPath);
            }

            foreach (var pick in FindObjectsOfType<Pick>())
            {
                foreach (var child in pick.GetComponentsInChildren<Transform>())
                {
					if (child.name.EndsWith(augmentation.poi))
                        {
							Destroy(child.gameObject);
							pick.ArrowRenderer.enabled = true;
					}

                }	
            }
        }



		private void DeletePickandPlaceData(string poi)
        {
			var jsonPath = System.IO.Path.Combine(Path, "pickandplaceinfo/" + poi + ".json");

			if (File.Exists(jsonPath))
			{
				//delete the json
				File.Delete(jsonPath);
			}
		}


		private void DeleteAllFilesInDirectory(string directoryName)
		{
			DirectoryInfo dir = new DirectoryInfo(directoryName);

			foreach (FileInfo f in dir.GetFiles())
			{
				f.Delete();
			}

			foreach (DirectoryInfo d in dir.GetDirectories())
			{
				DeleteAllFilesInDirectory(d.FullName);
				d.Delete();
			}
		}

		private void DeleteAnnotationFile(ToggleObject annotation)
		{
			// clean up associated file
			string filePath = annotation.url;
			const string httpPrefix = "http://";
			if (filePath.StartsWith(httpPrefix))
			{
				filePath = filePath.Remove(0, httpPrefix.Length);
			}
			string localFilePath = System.IO.Path.Combine(Path, System.IO.Path.GetFileName(filePath));
			if (File.Exists(localFilePath))
			{
				File.Delete(localFilePath);
			}
		}

		private void RegenerateActionsList()
		{
			ActionsOfTypeAction.Clear();
			for (int i = 0; i < Activity.actions.Count; i++)
			{
				if (Activity.actions[i].type == "action")
				{
					ActionsOfTypeAction.Add(Activity.actions[i]);
				}
			}
		}


		public void SaveData()
		{
			mustCleanUp = false;
			var recFilePath = System.IO.Path.Combine(Application.persistentDataPath, $"{Activity.id}-activity.json");
			var json = JsonUtility.ToJson(Activity);
			File.WriteAllText(recFilePath, json);
			WorkplaceManager.Instance.SaveWorkplace();

			SaveCharactersJsons();
			SavePickandPlaceJsons();
		}


		private static void  SaveCharactersJsons()
        {
			//save existing characters data
			foreach (var character in Resources.FindObjectsOfTypeAll<CharacterController>())
				character.SaveJson();
		}


		void SavePickandPlaceJsons()
        {
			//save exsiting pickandplace data
			foreach (PickAndPlaceController pickandplace in Resources.FindObjectsOfTypeAll<PickAndPlaceController>())
				pickandplace.SavePositions();
        }


		public void GenerateNewId(bool forceGenerateNewID = false)
		{
			if (newIdGenerated && !forceGenerateNewID)
			{
				return;
			}

			string id;
			if (!string.IsNullOrEmpty(Activity.id) && !forceGenerateNewID)
            {
				id = Activity.id;
            }
			else
            {
				string oldId = Activity.id;
				id = $"session-{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}";
				Activity.id = id;
				Activity.version = Application.version;
				string newDirectoryPath = System.IO.Path.Combine(Application.persistentDataPath, id);

				if (!Directory.Exists(newDirectoryPath))
					Directory.CreateDirectory(newDirectoryPath);

				if (!string.IsNullOrEmpty(oldId))
				{
					string sourcePath = System.IO.Path.Combine(Application.persistentDataPath, oldId);
					string targetPath = System.IO.Path.Combine(newDirectoryPath);
					CopyEntireFolder(sourcePath, targetPath);
				}

				Path = newDirectoryPath;
				mustCleanUp = true; // not yet saved; this is used if the user quits without saving so that we can clean up
			}

			string workplaceId = $"{id}-workplace.json";
			WorkplaceManager.Instance.Workplace.id = workplaceId;
			Activity.workplace = workplaceId;
			newIdGenerated = true;
		}


		private static void CopyEntireFolder(string folderPath, string destinationPath)
        {
            try
            {
				//Now Create all of the directories
				foreach (string dirPath in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
					Directory.CreateDirectory(dirPath.Replace(folderPath, destinationPath));

				//Copy all the files & Replaces any files with the same name
				foreach (string newPath in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
					File.Copy(newPath, newPath.Replace(folderPath, destinationPath), true);
			}
            catch (IOException e)
            {
				Debug.LogError(e);
            }
        }

		//public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
		//{
		//	Directory.CreateDirectory(target.FullName);

		//	// Copy each file into the new directory.
		//	foreach (FileInfo fi in source.GetFiles())
		//	{
		//		Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
		//		fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name), false);
		//	}

		//	// Copy each subdirectory using recursion.
		//	foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
		//	{
		//		DirectoryInfo nextTargetSubDir =
		//			target.CreateSubdirectory(diSourceSubDir.Name);
		//		CopyAll(diSourceSubDir, nextTargetSubDir);
		//	}
		//}

		private void CleanUp()
		{
			string newDirectoryPath = System.IO.Path.Combine(Application.persistentDataPath, Activity.id);
			if (Directory.Exists(newDirectoryPath))
			{
				Directory.Delete(newDirectoryPath, true);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MirageXR
{
    [Serializable]
    public class Activity
    {
        public string id;
        public string name;
        public string version;
        public string description;
        public string language;
        public string workplace;
        public string start;

        public List<Action> actions = new List<Action>();
    }

    [Serializable]
    public class Action // is a task station
    {
        // NOT IN ARLEM SPEC. For storing current action state.
        public bool isActive;
        public bool isCompleted;

        public string id;
        public string viewport;
        public ActionType type;
        public string device;
        public string location;
        public string predicate;
        public string user;

        public Instruction instruction;
        public List<string> appIDs = new List<string>();

        public Enter enter;
        public Exit exit;

        public List<Trigger> triggers;

        public void AddArlemTrigger(TriggerMode mode, ActionType type = ActionType.Action, string id = "start", float duration = 0f, string value = "")
        {
            // add a trigger, give it an id
            var trigger = new Trigger
            {
                id = id,    // ("TR-" + System.Guid.NewGuid().ToString()); // need a method to make this accessable to the user
                mode = mode,
                type = type,
                viewport = "actions",
                duration = duration,
                value = value
            };

            triggers.Add(trigger);
        }

        public void AddOrReplaceArlemTrigger(TriggerMode mode, ActionType type, string id, float duration, string value)
        {
            var trigger = triggers.Find(t => t.id == id);
            if (trigger != null)
            {
                trigger.mode = mode;
                trigger.type = type;
                trigger.id = id;
                trigger.duration = duration;
                trigger.value = value;
                return;
            }

            AddArlemTrigger(mode, type, id, duration, value);
        }

        public void RemoveArlemTrigger(ToggleObject toggleObject)
        {
            if (toggleObject == null) return;
            var myTrigger = triggers.Find(t => t.id == toggleObject.poi);
            if (myTrigger != null)
            {
                triggers.Remove(myTrigger);
            }
        }
        public void RemoveArlemTrigger(string triggerID)
        {
            var myTrigger = triggers.Find(t => t.id == triggerID);
            if (myTrigger != null)
            {
                triggers.Remove(myTrigger);
            }
        }
    }

    [Serializable]
    public abstract class EnterExit
    {
        public List<Message> messages = new List<Message>();
        public List<ToggleObject> activates = new List<ToggleObject>();
        public List<ToggleObject> deactivates = new List<ToggleObject>();
    }

    [Serializable]
    public class Enter : EnterExit { }

    [Serializable]
    public class Exit : EnterExit { }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionType
    {
        [EnumMember]
        Unknown,
        [EnumMember(Value = "action")]
        Action,
        [EnumMember(Value = "reaction")]
        Reaction,
        [EnumMember(Value = "tangible")]
        Tangible,
        [EnumMember(Value = "audio")]
        Audio,
        [EnumMember(Value = "video")]
        Video,
        [EnumMember(Value = "act")]
        Act,
        [EnumMember(Value = "label")]
        Label,
        [EnumMember(Value = "char")]
        Character,
        [EnumMember(Value = "pickandplace")]
        PickAndPlace,
        [EnumMember(Value = "none")]
        None,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerMode
    {
        [EnumMember]
        Unknown,
        [EnumMember(Value = "click")]
        Click,
        [EnumMember(Value = "character")]
        Character,
        [EnumMember(Value = "audio")]
        Audio,
        [EnumMember(Value = "video")]
        Video,
        [EnumMember(Value = "voice")]
        Voice,
        [EnumMember(Value = "sensor")]
        Sensor,
        [EnumMember(Value = "detect")]
        Detect,
        [EnumMember(Value = "pickandplace")]
        PickAndPlace,
        [EnumMember(Value = "incorrectpickandplace")]
        IncorrectPickAndPlace,
    }

    [Serializable]
    public class ToggleObject // is an annotation
    {
        public string id = string.Empty;          // the id of the thing/person/place (task station)
        public ActionType type;
        public string predicate = string.Empty;
        public string poi = string.Empty;         // this is an Annotation ID
        public string option = string.Empty;
        public string viewport = string.Empty;
        public string url = string.Empty;
        public string state = string.Empty;
        public string text = string.Empty;
        public string sensor = string.Empty;
        public string key = string.Empty;
        public string tangible = string.Empty;
        public string warning = string.Empty;

        // NOT IN ARLEM SPEC. For activated content fine tuning.
        public string position = string.Empty; // Vector3 format 0, 0, 0
        public string rotation = string.Empty; // Vector3 format 0, 0, 0
        public float scale = 0f;
        public bool positionLock = false;

        // NOT IN ARLEM SPEC. For the detect symbol.
        public float duration = 0f;

        // NOT IN ARLEM SPEC. For forcing the guide on.
        public bool guide;
    }

    [Serializable]
    public class Trigger
    {
        public TriggerMode mode;
        public ActionType type;
        public string viewport;
        public string id;
        public float duration;
        public string data;
        public string value;

        // NOT IN ARLEM SPEC. ARLEM spec uses "operator", which is already reserved by Unity.
        public string option = "";

        public static void SetupTriggers(Action action)
        {
            foreach (var trigger in action.triggers)
            {
                switch (trigger.mode)
                {
                    // Every trigger need to have a mode.
                    case TriggerMode.Unknown:
                        {
                            throw new ArgumentException("Trigger mode not set.");
                        }
                    case TriggerMode.Sensor:
                        {
                            var smartObj = Utilities.CreateObject($"{action.id}_smartTrigger_{trigger.id}", "Triggers");

                            if (smartObj == null)
                            {
                                throw new MissingComponentException("Couldn't create the smart trigger object.");
                            }

                            var smartBehaviour = smartObj.AddComponent<SmartTrigger>();

                            if (!smartBehaviour.CreateTrigger(action.id, trigger))
                            {
                                Object.Destroy(smartObj);
                                throw new MissingComponentException("Couldn't create the smart trigger.");
                            }

                            smartBehaviour.Activate();
                            break;
                        }
                }
            }
        }

        public static void DeleteTriggersForId(string id) // TODO: move it to TriggerManager
        {
            foreach (Transform trigger in GameObject.Find("Triggers").transform)
            {
                if (trigger.gameObject.name.StartsWith(id))
                {
                    Object.Destroy(trigger.gameObject);
                }
            }
        }
    }

    [Serializable]
    public class Message
    {
        public string target;
        public string channel;
        public string launch;
        public string text;
    }

    [Serializable]
    public class Instruction
    {
        public string title;
        public string description;
    }
}
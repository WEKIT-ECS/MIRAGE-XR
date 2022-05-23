using System;
using System.Collections.Generic;

namespace MirageXR
{
    [Serializable]
    public class Activity
    {
        public string id = "";
        public string name = "";
        public string version = "";
        public string description = "";
        public string language = "";
        public string workplace = "";
        public string start = "";

        public List<Action> actions = new List<Action> ();

        // NOT IN ARLEM SPEC. Activity welcome message.
        public string message = "";

        // NOT IN ARLEM SPEC. Used for defining if the activity will resume from last known position or from the start every time.
        public bool resumeable = true;
    }

    [Serializable]
    public class Action         // is a task station
    {
        // NOT IN ARLEM SPEC. For storing current action state.
        public bool isActive = false;
        public bool isCompleted = false;

        public string id = "";
        public string viewport = "";
        public string type = "";
        public string device = "";
        public string location = "";
        public string predicate = "";
        public string user = "";

        public Instruction instruction;
        public List<string> appIDs = new List<string>();

        public Enter enter;
        public Exit exit;

        public List<Trigger> triggers;  
        
        public void AddArlemTrigger(string mode, string type = "action", string id = "start", float duration = 0f, string value = "")
        {
            // add a trigger, give it an id
            var trigger = new Trigger
            {
                id = id, // ("TR-" + System.Guid.NewGuid().ToString());
                // need a method to make this accessable to the user
                mode = mode,
                type = type,
                viewport = "actions",
                duration = duration,
                value = value
            };

            triggers.Add(trigger);
        }
        
        public void AddOrReplaceArlemTrigger(string mode, string type, string id, float duration, string value)
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
    }

    [Serializable]
    public abstract class EnterExit
    {
        public bool removeSelf = false;
        public List<Message> messages = new List<Message> ();
        public List<ToggleObject> activates = new List<ToggleObject> ();
        public List<ToggleObject> deactivates = new List<ToggleObject> ();
    }

    [Serializable]
    public class Enter : EnterExit { }

    [Serializable]
    public class Exit : EnterExit { }

    [Serializable]
    public class ToggleObject      // is an annotation
    {
        public string id = "";          // the id of the thing/person/place (task station)
        public string type = "";
        public string predicate = "";
        public string poi = "";         // this is an Annotation ID
        public string option = "";
        public string viewport = "";
        public string url = "";
        public string state = "";
        public string text = "";
        public string sensor = "";
        public string key = "";
        public string tangible = "";
        public string warning = "";

        // NOT IN ARLEM SPEC. For activated content fine tuning.
        public string position = ""; // Vector3 format 0, 0, 0
        public string rotation = ""; // Vector3 format 0, 0, 0
        public float scale = 0f;

        // NOT IN ARLEM SPEC. For the detect symbol.
        public float duration = 0f;

        // NOT IN ARLEM SPEC. For forcing the guide on.
        public bool guide;
    }

    [Serializable]
    public class Activate : ToggleObject { }

    [Serializable]
    public class Deactivate : ToggleObject { }

    [Serializable]
    public class Trigger
    {
        public string mode = "";
        public string type = "";
        public string viewport = "";
        public string id = "";
        public float duration = 0f;
        public string data = "";
        public string value = "";

        // NOT IN ARLEM SPEC. ARLEM spec uses "operator", which is already reserved by Unity.
        public string option = "";
    }

    [Serializable]
    public class Message
    {
        public string target = "";
        public string channel = "";
        public string launch = "";
        public string text = "";
    }

    [Serializable]
    public class Instruction
    {
        public string title = "";
        public string description = "";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTriggerManager : MonoBehaviour
{
    [System.Serializable]
    public class EventBlock
    {
        public Animator animator;
        public string eventName;
    }

    public EventBlock[] eventBlock;

    public void SendEvent(int val)
    {
        eventBlock[val].animator.SetTrigger(eventBlock[val].eventName);
    }
}

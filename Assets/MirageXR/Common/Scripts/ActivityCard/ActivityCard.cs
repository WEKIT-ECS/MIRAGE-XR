using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityCard : MonoBehaviour
    {
        // Define all the components.

        private AudioSource _audio;

        // Main title shown on the top of the card.
        private Text _title;

        // Instruction text.
        private Text _instruction;
        
        // Incompleted icon.
        private GameObject _incompletedIcon;

        // Completed icon.
        private GameObject _completedIcon;

        // Touchable icon.
        private GameObject _touchableIcon;

        // Voice triggerable icon.
        private GameObject _voiceableIcon;

        // IoT triggerable icon.
        private GameObject _iotIcon;

        // Activity card manager.
        public ActivityCardManager Manager;

        public GameObject ContentPanel;

        public bool Voiceable;
        public bool Clickable;
        public bool Iotable;

        // Click behaviours
        [Serializable]
        public enum ClickBehaviours
        {
            ShowContent,
            Next,
            Previous,
            Last,
            First
        }

        // Click behaviour for this card.
        public ClickBehaviours ClickBehaviour = ClickBehaviours.ShowContent;

        // Action link.
        public Action ActionObject;

        // Is current card for the active step or not?
        public bool IsActiveStep;


        private void OnEnable()
        {
            EventManager.OnNext += Next;
            EventManager.OnPrevious += Previous;
            EventManager.OnClearAll += ClearAll;

            EventManager.OnBackByVoice += BackByVoice;
        }

        private void OnDisable()
        {
            EventManager.OnNext -= Next;
            EventManager.OnPrevious -= Previous;
            EventManager.OnClearAll -= ClearAll;

            EventManager.OnBackByVoice -= BackByVoice;
        }

        /// <summary>
        /// Standard Unity start method.
        /// </summary>
        private void Start()
        {
            // Populate all the variables.
            _audio = transform.parent.GetComponent<AudioSource>();
            _title = transform.FindDeepChild("Title").GetComponent<Text>();
            _instruction = transform.FindDeepChild("Instruction").GetComponent<Text>();
            _incompletedIcon = transform.FindDeepChild("IncompletedIcon").gameObject;
            _completedIcon = transform.FindDeepChild("CompletedIcon").gameObject;
            _touchableIcon = transform.FindDeepChild("TouchableIcon").gameObject;
            _voiceableIcon = transform.FindDeepChild("VoiceableIcon").gameObject;
            _iotIcon = transform.FindDeepChild("IoTIcon").gameObject;

        }

        private void ClearAll()
        {
            ActionObject = null;

            Clear();
        }

        private void Clear()
        {
            _title.text = "";
            _instruction.text = "";

            _incompletedIcon.SetActive(true);
            _completedIcon.SetActive(false);
            _touchableIcon.SetActive(false);
            _voiceableIcon.SetActive(false);
            _iotIcon.SetActive(false);

            Clickable = false;
            Voiceable = false;
            Iotable = false;
        }

        /// <summary>
        /// Set up an activity card when action has changed.
        /// </summary>
        /// <param name="action"></param>
        public void SetupCard(Action action)
        {
            // Link action.
            ActionObject = action;

            // Clear out everything first.
            Clear();

            // Then start filling crap in.
            _title.text = action.instruction.title;
            _instruction.text = action.instruction.description;
            
            _completedIcon.SetActive(action.isCompleted);
            _incompletedIcon.SetActive(!action.isCompleted);

            if (IsActiveStep)
            {
                // By default, the user cannot complete a task by clicking.
                _incompletedIcon.GetComponent<Button>().enabled = false;
            }


            // Populate feature panel.
            foreach (var trigger in action.triggers)
            {
                // Click trigger handling
                if (trigger.mode.Equals("click"))
                {
                    _touchableIcon.SetActive(true);
                    Clickable = true;

                    if (IsActiveStep)
                    {
                        // Now the user can complete an action by clicking the check box.
                        _incompletedIcon.GetComponent<Button>().enabled = true;
                    }
                }

                // Voice trigger handling.
                else if (trigger.mode.Equals("voice"))
                {
                    _voiceableIcon.SetActive(true);
                    Voiceable = true;
                }

                else if (trigger.mode.Equals("sensor"))
                {
                    _iotIcon.SetActive(true);
                    Iotable = true;
                }

            }
        }

        public void Next(string trigger)
        {
            if (IsActiveStep)
            {
                switch (trigger)
                {
                    case "voice":
                        // Moved to ActivityCardManager.cs...
                        break;

                    case "touch":
                        if (Clickable)
                        {
                            EventManager.Click();
                            Manager.Next();
                        }

                        else
                        {
                            EventManager.Click();
                            Maggie.Speak("Action doesn't contain a click trigger.");
                        }
                        break;
                }
            }   
        }

        public void NextTouch(string trigger)
        {
            EventManager.Click();
            Next(trigger);
        }

        private void Previous(string trigger)
        {
            if (!IsActiveStep && ClickBehaviour == ClickBehaviours.Previous)
            {
                switch (trigger)
                {
                    case "voice":
                        if (Iotable)
                            Maggie.Speak("Can't go back because of a smart trigger in previous step.");
                        break;
                    case "touch":
                        if (Iotable)
                        {
                            EventManager.Click();
                            Maggie.Speak("Can't go back because of a smart trigger in previous step.");
                        }
                        else
                        {
                            EventManager.Click();
                            Manager.Previous();
                        }
                        break;
                }
            }
        }

        public void ClickActionTouch()
        {
            // Show next card when active card is clicked 
            if (IsActiveStep && ClickBehaviour == ClickBehaviours.Next)
            {
                EventManager.Next("touch");
            }
            // Force activate currently inactive action.
            else
            {
                // Next card.
                if (!IsActiveStep && ClickBehaviour == ClickBehaviours.Next)
                {
                    EventManager.Next("touch");
                }

                else if (!IsActiveStep && ClickBehaviour == ClickBehaviours.Previous)
                {
                    EventManager.Previous("touch");
                }
            }
        }

        public void NextByVoice()
        {
            // Force activate currently inactive action.
            if (!IsActiveStep)
            {
                // Next card.
                if (!IsActiveStep && ClickBehaviour == ClickBehaviours.Next)
                {
                    EventManager.Next("voice");
                }
            }
        }

        public void BackByVoice()
        {
            // Force activate currently inactive action.
            if (!IsActiveStep)
            {
                if (!IsActiveStep && ClickBehaviour == ClickBehaviours.Previous)
                {
                    EventManager.Previous("voice");
                }
            }
        }

    }
}
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

        private AudioSource audio;

        // Main title shown on the top of the card.
        private Text title;

        // Instruction text.
        private Text instruction;

        // Incompleted icon.
        private GameObject incompletedIcon;

        // Completed icon.
        private GameObject completedIcon;

        // Touchable icon.
        private GameObject touchableIcon;

        // Voice triggerable icon.
        private GameObject voiceableIcon;

        // IoT triggerable icon.
        private GameObject iotIcon;

        // Activity card manager.
        [SerializeField] private ActivityCardManager Manager;

        [SerializeField] private GameObject ContentPanel;

        [SerializeField] private bool voiceable;
        [SerializeField] private bool clickable;
        [SerializeField] private bool iotable;

        public bool Voiceable => voiceable;
        public bool Iotable => iotable;

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
        [SerializeField] private ClickBehaviours ClickBehaviour = ClickBehaviours.ShowContent;

        // Action link.
        [SerializeField] private Action actionObject;

        public Action ActionObject => actionObject;

        // Is current card for the active step or not?
        [SerializeField] private bool IsActiveStep;


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
            audio = transform.parent.GetComponent<AudioSource>();
            title = transform.FindDeepChild("Title").GetComponent<Text>();
            instruction = transform.FindDeepChild("Instruction").GetComponent<Text>();
            incompletedIcon = transform.FindDeepChild("IncompletedIcon").gameObject;
            completedIcon = transform.FindDeepChild("CompletedIcon").gameObject;
            touchableIcon = transform.FindDeepChild("TouchableIcon").gameObject;
            voiceableIcon = transform.FindDeepChild("VoiceableIcon").gameObject;
            iotIcon = transform.FindDeepChild("IoTIcon").gameObject;

        }

        private void ClearAll()
        {
            actionObject = null;

            Clear();
        }

        private void Clear()
        {
            title.text = "";
            instruction.text = "";

            incompletedIcon.SetActive(true);
            completedIcon.SetActive(false);
            touchableIcon.SetActive(false);
            voiceableIcon.SetActive(false);
            iotIcon.SetActive(false);

            clickable = false;
            voiceable = false;
            iotable = false;
        }

        /// <summary>
        /// Set up an activity card when action has changed.
        /// </summary>
        /// <param name="action"></param>
        public void SetupCard(Action action)
        {
            // Link action.
            actionObject = action;

            // Clear out everything first.
            Clear();

            // Then start filling crap in.
            title.text = action.instruction.title;
            instruction.text = action.instruction.description;

            completedIcon.SetActive(action.isCompleted);
            incompletedIcon.SetActive(!action.isCompleted);

            if (IsActiveStep)
            {
                // By default, the user cannot complete a task by clicking.
                incompletedIcon.GetComponent<Button>().enabled = false;
            }


            // Populate feature panel.
            foreach (var trigger in action.triggers)
            {
                // Click trigger handling
                if (trigger.mode.Equals("click"))
                {
                    touchableIcon.SetActive(true);
                    clickable = true;

                    if (IsActiveStep)
                    {
                        // Now the user can complete an action by clicking the check box.
                        incompletedIcon.GetComponent<Button>().enabled = true;
                    }
                }

                // Voice trigger handling.
                else if (trigger.mode.Equals("voice"))
                {
                    voiceableIcon.SetActive(true);
                    voiceable = true;
                }

                else if (trigger.mode.Equals("sensor"))
                {
                    iotIcon.SetActive(true);
                    iotable = true;
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
                        if (clickable)
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
                        if (iotable)
                            Maggie.Speak("Can't go back because of a smart trigger in previous step.");
                        break;
                    case "touch":
                        if (iotable)
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
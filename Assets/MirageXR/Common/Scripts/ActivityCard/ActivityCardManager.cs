using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityCardManager : MonoBehaviour
    {
        // Variables.
        [SerializeField] private GameObject FirstCard;

        [SerializeField] private GameObject PreviousCard;
        [SerializeField] private GameObject ActiveCard;
        [SerializeField] private GameObject NextCard;
        [SerializeField] private GameObject LastCard;

        [SerializeField] private RectTransform ContentPanel;
        [SerializeField] private GameObject ShowCardsToggle;

        [SerializeField] private int _actionIndex;

        [SerializeField] private RectTransform TimeLine;
        [SerializeField] private List<GameObject> _timelineObjects = new List<GameObject>();
        [SerializeField] private int _timelineIndex;

        [SerializeField] private Text TasklistTitle;
        [SerializeField] private RectTransform Tasklist;
        [SerializeField] private List<GameObject> _tasklistObjects = new List<GameObject>();

        [SerializeField] private bool _isFirstRun = true;

        [SerializeField] private string ActivityId = "";
        [SerializeField] private List<Action> OriginalActions = new List<Action>();
        [SerializeField] private List<Action> Actions = new List<Action>();

        [SerializeField] private GameObject FinishFlag;
        [SerializeField] private GameObject ReplayButton;

        [SerializeField] private bool IsResumeable;

        private void OnEnable()
        {
            EventManager.OnClearAll += Reset;
            EventManager.OnInitUi += Init;
        }

        private void OnDisable()
        {
            EventManager.OnClearAll -= Reset;
            EventManager.OnInitUi -= Init;
        }

        private void Reset()
        {
            TasklistTitle.text = "ACTIVITY TITLE";

            FinishFlag.SetActive(false);
            ReplayButton.SetActive(false);

            _isFirstRun = true;

            ActivityId = "";
            _timelineObjects.Clear();
            _tasklistObjects.Clear();
            OriginalActions.Clear();
            Actions.Clear();
        }

        public void Start()
        {
            // Hide last card specific items in start.
            FinishFlag.SetActive(false);
            ReplayButton.SetActive(false);
        }

        public void Init()
        {
            // Adapt to stubby activities...
            if (Actions.Count < 5)
            {
                LastCard.SetActive(false);
                FirstCard.SetActive(false);
                if (Actions.Count < 3)
                {
                    NextCard.SetActive(false);
                    PreviousCard.SetActive(false);
                }
            }

            else
            {
                LastCard.SetActive(true);
                NextCard.SetActive(true);
                PreviousCard.SetActive(true);
                FirstCard.SetActive(true);
            }

            foreach (var action in Actions)
            {
                // Skip reactions.
                if (action.type.Equals("reaction"))
                    continue;

                var timelineobject = Instantiate(Resources.Load<GameObject>("Prefabs/TimelineItemPrefab"), Vector3.zero,
                    Quaternion.identity, TimeLine);
                timelineobject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                timelineobject.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
                timelineobject.GetComponent<RectTransform>().localScale = Vector3.one;
                timelineobject.name = "Step-" + action.id;
                _timelineObjects.Add(timelineobject);

                var tasklistobject = Instantiate(Resources.Load<GameObject>("Prefabs/UI/TaskStepPrefab"), Vector3.zero,
                    Quaternion.identity, Tasklist);
                tasklistobject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                tasklistobject.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
                tasklistobject.GetComponent<RectTransform>().localScale = Vector3.one;
                tasklistobject.name = "Step-" + action.id;

                tasklistobject.GetComponent<TaskStep>().SetupStep(action);
                tasklistobject.GetComponent<TaskStep>().IsActive = false;

                _tasklistObjects.Add(tasklistobject);
            }

            // Reset all timeline indicators.
            foreach (var timelineObject in _timelineObjects)
            {
                timelineObject.transform.FindDeepChild("ActiveIndicator").GetComponent<Image>().enabled = false;
            }

            // Reset all tasklist objects.
            foreach (var task in _tasklistObjects)
            {
                task.GetComponent<TaskStep>().IsActive = false;
            }

            _isFirstRun = false;
        }

        public void ActionChange(string actionId)
        {
            if (_isFirstRun)
            {
                Init();

                _isFirstRun = false;
            }

            // For getting action index in the action card modified list.
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i].id.Equals(actionId))
                    _actionIndex = i;
            }

            // For getting the timeline object index.
            for (int i = 0; i < OriginalActions.Count; i++)
            {
                if (OriginalActions[i].id.Equals(actionId))
                    _timelineIndex = i;
            }

            // Show last card specific items if last card is active.
            if (_timelineIndex.Equals(OriginalActions.Count - 1))
            {
                FinishFlag.SetActive(true);
                ReplayButton.SetActive(true);

                EventManager.ActivityCompletedStamp(SystemInfo.deviceUniqueIdentifier, ActivityManager.Instance.Activity.id, DateTime.UtcNow.ToUniversalTime().ToString());

                // Set restore state to starting action.
                PlayerPrefs.SetString(ActivityId, "StartingAction");
            }

            // Otherwise hide the last card specific items.
            else
            {
                FinishFlag.SetActive(false);
                ReplayButton.SetActive(false);

                if (IsResumeable)
                    // Set restore state to active action.
                    PlayerPrefs.SetString(ActivityId, actionId);
                else
                    // Set restore state to starting action.
                    PlayerPrefs.SetString(ActivityId, "StartingAction");
            }

            // Reset all timeline indicators.
            foreach (var timelineObject in _timelineObjects)
            {
                timelineObject.transform.FindDeepChild("ActiveIndicator").GetComponent<Image>().enabled = false;
            }

            // Reset all tasklist objects.
            foreach (var task in _tasklistObjects)
            {
                task.GetComponent<TaskStep>().IsActive = false;
            }

            // Activate timeline indicator for the active step only.
            _timelineObjects[_timelineIndex].transform.FindDeepChild("ActiveIndicator").GetComponent<Image>().enabled =
                true;

            // Activate tasklist object.
            _tasklistObjects[_timelineIndex].GetComponent<TaskStep>().IsActive = true;

            // Shuffle actions list if needed (not moving simply to the next one...).
            for (int i = 0; i < _actionIndex; i++)
            {
                MoveCard();
            }

            // First setup the active card.
            ActiveCard.GetComponent<ActivityCard>().SetupCard(Actions[0]);

            if (Actions.Count > 1)
            {
                // Then the next one.
                NextCard.GetComponent<ActivityCard>().SetupCard(Actions[1]);

                // The previous one.
                PreviousCard.GetComponent<ActivityCard>().SetupCard(Actions[Actions.Count - 1]);

                // The first and last cards are only used when there are atleast 5 cards.
                if (Actions.Count < 5)
                {
                    FirstCard.SetActive(false);
                    LastCard.SetActive(false);
                }

                // If atleast 5 cards...
                else
                {
                    FirstCard.SetActive(true);
                    LastCard.SetActive(true);

                    FirstCard.GetComponent<ActivityCard>().SetupCard(Actions[Actions.Count - 2]);
                    LastCard.GetComponent<ActivityCard>().SetupCard(Actions[2]);
                }
                // Now move the first index to last.
                MoveCard();
            }
            

            // Finally make sure that all the cards are visible...
            ShowCards();
        }

        // Moves the first action to the end of the list.
        private void MoveCard()
        { 
            var moveItem = Actions[0];
            Actions.RemoveAt(0);
            Actions.Add(moveItem);
        }

        /// <summary>
        /// Activate next action.
        /// </summary>
        public void Next()
        {
            var actionObject = ActiveCard.GetComponent<ActivityCard>().ActionObject;
            actionObject.isCompleted = true;
            EventManager.DeactivateAction(actionObject.id);
        }

        public void NextByVoice()
        {
            if (ActiveCard.GetComponent<ActivityCard>().Voiceable)
            {
                Maggie.Ok();
                Next();
            }
            else
            {
                Maggie.Speak("Action doesn't contain a voice trigger.");
            }
        }

        /// <summary>
        /// Force activate previous action.
        /// </summary>
        public void Previous()
        {
            var actionObject = PreviousCard.GetComponent<ActivityCard>().ActionObject;
            EventManager.GoBack(actionObject.id);
        }

        public void BackByVoice()
        {
            if (!PreviousCard.GetComponent<ActivityCard>().Iotable)
            {
                Maggie.Ok();
                Previous();
            }
            else
                Maggie.Speak("Can't go back because of a smart trigger in previous step.");
        }

        public void ShowCards()
        { 
            if (Actions.Count > 1)
            {
                NextCard.SetActive(true);
                if (Actions.Count > 2)
                {
                    PreviousCard.SetActive(true);

                    if (Actions.Count > 4)
                    {
                        FirstCard.SetActive(true);
                        LastCard.SetActive(true);
                    }
                }
            }
            ActiveCard.SetActive(true);

            // ContentPanel.localScale = Vector3.zero; // Moved this to HideContentPanel
            ShowCardsToggle.SetActive(false);

            var audioObjects = ContentPanel.gameObject.transform.GetComponentsInChildren<AudioSource>();
            foreach (var audioObject in audioObjects)
            {
                audioObject.Stop();
            }
        }

        public void HideCards()
        {
            FirstCard.SetActive(false);
            PreviousCard.SetActive(false);
            ActiveCard.SetActive(false);
            NextCard.SetActive(false);
            LastCard.SetActive(false);

            ShowCardsToggle.SetActive(true); // Does this need to go in function below?
        }

        public void ShowContentPanel()
        {
            ContentPanel.gameObject.SetActive(true);
            ContentPanel.localScale = Vector3.one;
        }

        public void HideContentPanel()
        {
            ContentPanel.gameObject.SetActive(false);
            ContentPanel.localScale = Vector3.zero;
        }

        public void ShowCardsVoice()
        {
            Maggie.Ok();
            ShowCards();
        }

        public void HideCardsVoice()
        {
            Maggie.Ok();
            HideCards();
            ShowContentPanel();
        }

        public void ShowCardsTouch()
        {
            EventManager.Click();
            ShowCards();
        }

        public void HideCardsTouch()
        {
            EventManager.Click();
            HideCards();
            ShowContentPanel();
        }
    }
}
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine;
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
        [SerializeField] private RectTransform TimeLine;
        [SerializeField] private List<GameObject> _timelineObjects = new List<GameObject>();
        [SerializeField] private Text TasklistTitle;
        [SerializeField] private RectTransform Tasklist;
        [SerializeField] private List<GameObject> _tasklistObjects = new List<GameObject>();
        [SerializeField] private List<LearningExperienceEngine.Action> OriginalActions = new List<LearningExperienceEngine.Action>();
        [SerializeField] private List<LearningExperienceEngine.Action> Actions = new List<LearningExperienceEngine.Action>();
        [SerializeField] private GameObject FinishFlag;
        [SerializeField] private GameObject ReplayButton;

        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnClearAll += Reset;
            LearningExperienceEngine.EventManager.OnInitUi += Init;
        }

        private void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnClearAll -= Reset;
            LearningExperienceEngine.EventManager.OnInitUi -= Init;
        }

        private void Reset()
        {
            TasklistTitle.text = "ACTIVITY TITLE";

            FinishFlag.SetActive(false);
            ReplayButton.SetActive(false);

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

        private void Init()
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
                if (action.type == ActionType.Reaction)
                {
                    continue;
                }

                var timelineItemPrefab = Resources.Load<GameObject>("Prefabs/TimelineItemPrefab"); // TODO: change to addressables
                var timelineObject = Instantiate(timelineItemPrefab, Vector3.zero, Quaternion.identity, TimeLine);
                var timelineRectTransform = timelineObject.GetComponent<RectTransform>();
                timelineRectTransform.localPosition = Vector3.zero;
                timelineRectTransform.localEulerAngles = Vector3.zero;
                timelineRectTransform.localScale = Vector3.one;
                timelineObject.name = $"Step-{action.id}";
                _timelineObjects.Add(timelineObject);

                var taskStepPrefab = Resources.Load<GameObject>("Prefabs/UI/TaskStepPrefab");  // TODO: change to addressables
                var taskListObject = Instantiate(taskStepPrefab, Vector3.zero, Quaternion.identity, Tasklist);
                var taskListRectTransform = taskListObject.GetComponent<RectTransform>();
                taskListRectTransform.localPosition = Vector3.zero;
                taskListRectTransform.localEulerAngles = Vector3.zero;
                taskListRectTransform.localScale = Vector3.one;
                taskListObject.name = $"Step-{action.id}";
                Debug.LogInfo("[ActivityCardManager] Task station instantiated with name = " + taskListObject.name);

                taskListObject.GetComponent<TaskStep>().SetupStep(action);
                taskListObject.GetComponent<TaskStep>().IsActive = false;

                _tasklistObjects.Add(taskListObject);
            }

            // Reset all timeline indicators.
            foreach (var timelineObject in _timelineObjects)
            {
                timelineObject.transform.FindDeepChild("ActiveIndicator").GetComponent<Image>().enabled = false;
            }

            // Reset all task list objects.
            foreach (var task in _tasklistObjects)
            {
                task.GetComponent<TaskStep>().IsActive = false;
            }
        }

        /// <summary>
        /// Activate next action.
        /// </summary>
        public async void Next()
        {
            var actionObject = ActiveCard.GetComponent<ActivityCard>().ActionObject;
            actionObject.isCompleted = true;
            await LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.DeactivateAction(actionObject.id);
        }

        /// <summary>
        /// Force activate previous action.
        /// </summary>
        public void Previous()
        {
            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivatePreviousAction();
        }

        private void ShowCards()
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

        public void ShowCardsTouch()
        {
            LearningExperienceEngine.EventManager.Click();
            ShowCards();
        }
    }
}
using LearningExperienceEngine;
using System;
using MirageXR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Action = LearningExperienceEngine.Action;

public class ActionDetailView : MonoBehaviour
{
    private LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    [SerializeField] private InputField titleLabel;
    [SerializeField] private Text numberLabel;
    [SerializeField] private InputField descriptionText;
    [SerializeField] private Button RepositionButton;

    [SerializeField] private Transform poiPredicatesParent;
    [SerializeField] private GameObject poiPredicateItemPrefab;

    private LearningExperienceEngine.Action _displayedAction;

    private readonly List<AnnotationListItem> poiPredicateItems = new List<AnnotationListItem>();
    private ActionEditor editor;

    public LearningExperienceEngine.Action DisplayedAction
    {
        get
        {
            return _displayedAction;
        }
        set
        {
            _displayedAction = value;
            UpdateUI();
        }
    }

    public ActionEditor Editor
    {
        get
        {
            if (editor == null)
            {
                editor = GetComponent<ActionEditor>();
            }
            return editor;
        }
    }

    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnActivateAction += OnActionActivated;
        LearningExperienceEngine.EventManager.OnActionModified += OnActionChanged;
    }

    private void OnDisable()
    {
        LearningExperienceEngine.EventManager.OnActivateAction -= OnActionActivated;
        LearningExperienceEngine.EventManager.OnActionModified -= OnActionChanged;
    }

    private void OnActionChanged(Action action)
    {
        UpdateUI();
    }

    // move the taskcard to the task station position
    public void MoveEditorNextToTaskSTation() // TODO: fix method name
    {
        var taskStation = GetCurrentTaskStation();
        transform.SetParent(taskStation.transform);
        var editorStartPosition = taskStation.transform.Find("EditorStartPosition");
        if (editorStartPosition)
        {
            transform.position = editorStartPosition.position;
            transform.rotation = editorStartPosition.rotation;
        }
        else
        {
            Debug.LogError("Can't find EditorStartPosition object");
        }

        RepositionButton.gameObject.SetActive(false);
    }

    public GameObject GetCurrentTaskStation()
    {
        var actionId = activityManager.ActiveActionId;
        var place = GameObject.Find(actionId);
        if (place)
        {
            var taskStation = place.transform.Find("default/PlayerTaskStation(Clone)"); // TODO: possible NRE
            if (taskStation)
            {
                return taskStation.gameObject;
            } else
            {
                Debug.LogWarning("Could not find task station gameObject clone under this gameObject '" + actionId + "'");
            }

        } else
        {
            Debug.LogWarning("Could not find place with actionID = " + actionId);
        }

        throw new Exception($"Could not find PlayerTaskStation for {actionId}");
    }

    public void UpdateUI()
    {
        // If all action steps are deleted and only the last one exist, add it to displayedAction
        if (activityManager.ActionsOfTypeAction.Count == 1)
        {
            _displayedAction = activityManager.ActionsOfTypeAction[0];
        }

        if (_displayedAction == null)
        {
            titleLabel.text = string.Empty;
            descriptionText.text = string.Empty;
            numberLabel.text = "00";
        }
        else
        {
            // move the taskcard to the task station position
            MoveEditorNextToTaskSTation();

            titleLabel.text = DisplayedAction.instruction.title;
            descriptionText.text = DisplayedAction.instruction.description;

            List<LearningExperienceEngine.Action> actions = activityManager.ActionsOfTypeAction;

            int index = actions.IndexOf(actions.FirstOrDefault(p => p.id.Equals(DisplayedAction.id)));
            numberLabel.text = (index + 1).ToString("00");

            // fill or create all necessary item labels
            for (int i = 0; i < _displayedAction.enter.activates.Count; i++)
            {
                if (i < poiPredicateItems.Count)
                {
                    poiPredicateItems[i].gameObject.SetActive(true);
                    poiPredicateItems[i].SetUp(this, _displayedAction.enter.activates[i]);
                }
                else
                {
                    var poiItemInstance = Instantiate(poiPredicateItemPrefab, poiPredicatesParent);
                    var poiListItem = poiItemInstance.GetComponent<AnnotationListItem>();
                    poiListItem.SetUp(this, _displayedAction.enter.activates[i]);
                    poiListItem.OnAnnotationItemClicked += OnAnnotationSelected;
                    poiPredicateItems.Add(poiListItem);
                }
            }

            // Show Grid view if there is no augmentation yet
            ActionEditor.Instance.AddMenuVisible = !activityManager.ActiveAction.enter.activates.Any();

            // disable all unused item labels
            for (int i = _displayedAction.enter.activates.Count; i < poiPredicateItems.Count; i++)
            {
                poiPredicateItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnAnnotationSelected(LearningExperienceEngine.ToggleObject annotation)
    {
        Editor.EditAnnotation(annotation);
    }

    private void OnActionActivated(string actionId)
    {
        List<LearningExperienceEngine.Action> actions = activityManager.ActionsOfTypeAction;
        DisplayedAction = actions.FirstOrDefault(p => p.id.Equals(actionId));
    }

}

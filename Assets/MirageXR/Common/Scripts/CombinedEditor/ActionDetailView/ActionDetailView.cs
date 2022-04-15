using MirageXR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActionDetailView : MonoBehaviour
{
    [SerializeField] private InputField titleLabel;
    [SerializeField] private Text numberLabel;
    [SerializeField] private InputField descriptionText;
    [SerializeField] private Button RepositionButton;

    [SerializeField] private Transform poiPredicatesParent;
    [SerializeField] private GameObject poiPredicateItemPrefab;

    private Action displayedAction;

    private readonly List<AnnotationListItem> poiPredicateItems = new List<AnnotationListItem>();
    private ActionEditor editor;

    public Action DisplayedAction
    {
        get
        {
            return displayedAction;
        }
        set
        {
            displayedAction = value;
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
        EventManager.OnActivateAction += OnActionActivated;
        EventManager.OnActionModified += OnActionChanged;
    }

    private void OnDisable()
    {
        EventManager.OnActivateAction -= OnActionActivated;
        EventManager.OnActionModified -= OnActionChanged;
    }

    private void OnActionChanged(Action action)
    {
        UpdateUI();
    }

    //move the taskcard to the task station position
    public void MoveEditorNextToTaskSTation() //TODO: fix method name
    {
        GameObject activeTSdiamond = GetCurrentTaskStation();
        transform.SetParent(activeTSdiamond.transform);
        var editorStartPosition = activeTSdiamond.transform.Find("EditorStartPosition");
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
        var actionId = ActivityManager.Instance.ActiveActionId;
        var activeTSdiamond = GameObject.Find(actionId).transform.Find("default").transform.Find("PlayerTaskStation(Clone)").gameObject; //TODO: possible NRE 
        return activeTSdiamond;
    }

    public void UpdateUI()
    {
        //If all action steps are deleted and only the last one exist, add it to displayedAction
        if (ActivityManager.Instance.ActionsOfTypeAction.Count == 1)
            displayedAction = ActivityManager.Instance.ActionsOfTypeAction[0];

        if (displayedAction == null)
        {
            titleLabel.text = string.Empty;
            descriptionText.text = string.Empty;
            numberLabel.text = "00";
        }
        else
        {
            //move the taskcard to the task station position
            MoveEditorNextToTaskSTation();

            titleLabel.text = DisplayedAction.instruction.title;
            descriptionText.text = DisplayedAction.instruction.description;

            List<Action> actions = ActivityManager.Instance.ActionsOfTypeAction;

            int index = actions.IndexOf(actions.FirstOrDefault(p => p.id.Equals(DisplayedAction.id)));
            numberLabel.text = (index + 1).ToString("00");

            // fill or create all necessary item labels
            for (int i = 0; i < displayedAction.enter.activates.Count; i++)
            {
                if (i < poiPredicateItems.Count)
                {
                    poiPredicateItems[i].gameObject.SetActive(true);
                    poiPredicateItems[i].SetUp(this, displayedAction.enter.activates[i]);
                }
                else
                {
                    GameObject poiItemInstance = Instantiate(poiPredicateItemPrefab, poiPredicatesParent);
                    AnnotationListItem poiListItem = poiItemInstance.GetComponent<AnnotationListItem>();
                    poiListItem.SetUp(this, displayedAction.enter.activates[i]);
                    poiListItem.OnAnnotationItemClicked += OnAnnotationSelected;
                    poiPredicateItems.Add(poiListItem);
                }
            }

            if (displayedAction.enter.activates.Count == 0)
                ActionEditor.Instance.AddMenuVisible = true;

            // disable all unused item labels
            for (int i = displayedAction.enter.activates.Count; i < poiPredicateItems.Count; i++)
            {
                poiPredicateItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnAnnotationSelected(ToggleObject annotation)
    {
        Editor.EditAnnotation(annotation);
    }

    private void OnActionActivated(string actionId)
    {
        List<Action> actions = ActivityManager.Instance.ActionsOfTypeAction;
        DisplayedAction = actions.FirstOrDefault(p => p.id.Equals(actionId));
    }

}

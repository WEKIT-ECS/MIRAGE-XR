using MirageXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionListMenu : MonoBehaviour
{
    [SerializeField] private GameObject actionItemPrefab;
    [SerializeField] private RectTransform listViewParent;
    [SerializeField] private Button previousStepButton;
    [SerializeField] private Button nextStepButton;
    [SerializeField] private Text pageLabel;
    [SerializeField] private InputField titleText;
    public InputField TitleText => titleText;

    [SerializeField] private GameObject addActionStepButton;
    public GameObject AddActionStepButton => addActionStepButton;

    [HideInInspector]
    public GameObject uploadProgressText { get; private set; }

    public static ActionListMenu Instance { get; private set; }

    private int page;
    private int totalNumberOfPages;
    private const int itemsPerPage = 10;

    private List<Action> actions;
    public ActionListItem[] ActionListItems { get; private set; } = new ActionListItem[itemsPerPage];

    private int Page
    {
        get => page;
        set
        {
            page = Mathf.Clamp(value, 0, totalNumberOfPages - 1);
        }
    }

    private void Start()
    {
        if (!Instance)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        if (!PlatformManager.Instance.WorldSpaceUi)
        {
            GetComponent<Canvas>().enabled = false;
            return;
        }
        
        //hide the upload progress number
        if (uploadProgressText == transform.FindDeepChild("UploadProgress").gameObject)
            uploadProgressText.SetActive(false);


        for (int i = 0; i < itemsPerPage; i++)
        {
            GameObject itemInstance = Instantiate(actionItemPrefab, listViewParent);
            ActionListItems[i] = itemInstance.GetComponent<ActionListItem>();
        }

        if (ActivityManager.Instance.IsReady)
        {
            Init();

            //activate the first step
            //ActivityManager.Instance.ActivateByID(ActivityManager.Instance.ActionsOfTypeAction[0].id);
        }
        
        Debug.Log("Action list menu start called");
        EventManager.OnInitUi += Init;
        EventManager.OnActivateAction += OnActivateAction;
        EventManager.OnDeactivateAction += OnDeactivateAction;
        EventManager.OnActionCreated += OnActionCreated;
        EventManager.OnActionDeleted += OnActionDeleted;
        EventManager.OnActionModified += OnActionChanged;
        EventManager.OnNextByVoice += NextAction;
        EventManager.OnBackByVoice += PreviousAction;
    }

    private void OnDestroy()
    {
        EventManager.OnInitUi -= Init;
        EventManager.OnActivateAction -= OnActivateAction;
        EventManager.OnDeactivateAction -= OnDeactivateAction;
        EventManager.OnActionCreated -= OnActionCreated;
        EventManager.OnActionDeleted -= OnActionDeleted;
        EventManager.OnActionModified -= OnActionChanged;
        EventManager.OnNextByVoice -= NextAction;
        EventManager.OnBackByVoice -= PreviousAction;
    }

    private void Init()
    {
        page = 0;
        actions = ActivityManager.Instance.ActionsOfTypeAction;
        titleText.text = ActivityManager.Instance.Activity.name;

        this.titleText.onValueChanged.AddListener(delegate { EventManager.NotifyOnActivityRenamed(); });
        

        UpdateUI();

        this.addActionStepButton = this.gameObject.transform.Find("Panel").Find("ButtonAdd").gameObject;
    }


    private void UpdateUI()
    {
        totalNumberOfPages = (actions.Count - 1) / itemsPerPage > 0 ? itemsPerPage: 1;
        DisplayList();
        CheckStepButtons();
        UpdatePageLabel();
    }

    private void DisplayList()
    {
        int startIndexOnPage = itemsPerPage * page;
        int itemCount = Mathf.Min(itemsPerPage, actions.Count - startIndexOnPage);

        for (int i = 0; i < itemCount; i++)
        {
            int index = startIndexOnPage + i;
            ActionListItems[i].Content = actions[index];
            ActionListItems[i].DataIndex = index;
            ActionListItems[i].UpdateView();
        }

        // fill the rest with null data
        for (int i = itemCount; i < itemsPerPage; i++)
        {
            ActionListItems[i].Content = null;
            ActionListItems[i].UpdateView();
        }
    }

    private void CheckStepButtons()
    {
        int activeIndex = ActivityManager.Instance.ActionsOfTypeAction.IndexOf(ActivityManager.Instance.ActiveAction);
        previousStepButton.interactable = activeIndex > 0;

        bool isLastStep = activeIndex >= ActivityManager.Instance.ActionsOfTypeAction.Count - 1;

        if (isLastStep)
            ActivityManager.Instance.ActiveAction.isCompleted = true;

        bool isCompleted = ActivityManager.Instance.ActiveAction == null || ActivityManager.Instance.ActiveAction.isCompleted;
        nextStepButton.interactable = (!isLastStep || !isCompleted) && activeIndex < actions.Count-1;

        //automaticaly go to next page if step is on next page
        if (activeIndex + 1 > itemsPerPage * (page +1))
            NextPage();
        //automaticaly go to previous page if step is on previous page
        else if (activeIndex + 1 <= itemsPerPage * page)
            PreviousPage();

        UpdatePageLabel();

        StartCoroutine(ButtonShortTimeDeactivation(previousStepButton.interactable, nextStepButton.interactable));

        //if the last step is done, next time start again from the first step
        if (isLastStep && isCompleted)
        {
            PlayerPrefs.SetString(ActivityManager.Instance.Activity.id, "StartingAction");
            PlayerPrefs.Save();
        }

        //find the target annotation of this action
        FindNavigatorTarget();
    }

    IEnumerator ButtonShortTimeDeactivation(bool previousButtonIsActive, bool nextButtonIsActive)
    {
        previousStepButton.interactable = false;
        nextStepButton.interactable = false;
        yield return new WaitForSeconds(0.5f);
        previousStepButton.interactable = previousButtonIsActive;
        nextStepButton.interactable = nextButtonIsActive;
    }


    private void UpdatePageLabel()
    {
        int displayPageIndex = actions.IndexOf(ActivityManager.Instance.ActiveAction) + 1;
        int totalNumberOfAction = actions.Count;
        pageLabel.text = $"{displayPageIndex:00}/{totalNumberOfAction:00}";
    }


    private void FindNavigatorTarget()
    {
        var targetAnnotation = ActivityManager.Instance.ActiveAction.enter.activates.Find(a => a.state == "target");
        if (targetAnnotation != null)
        {
            Transform target = CorrectTargetObject(targetAnnotation);
            
            TaskStationDetailMenu.Instance.NavigatorTarget = target;
            TaskStationDetailMenu.Instance.TargetPredicate = targetAnnotation.predicate;
        }
        else
        {
            TaskStationDetailMenu.Instance.NavigatorTarget = null;
        }
    }


    public static Transform CorrectTargetObject(ToggleObject annotation)
    {
        Transform target;
        switch (annotation.predicate)
        {
            case string type when type.StartsWith("char"):
                target = GameObject.Find(annotation.poi).GetComponentInChildren<MirageXR.CharacterController>().transform;
                break;
            case string type when type.StartsWith("pick"):
                target = GameObject.Find(annotation.poi).GetComponentInChildren<PickAndPlaceController>().Target;
                break;
            case "ghosttracks":
                target = GameObject.Find(annotation.poi).GetComponentInChildren<GhostRecordPlayer>().transform;
                break;
            default:
                target = GameObject.Find(annotation.poi).transform;
                break;
        }

        return target;
    }



    private void OnActivateAction(string action)
    {
        CheckStepButtons();
    }

    private void OnDeactivateAction(string action, bool doNotActivateNextStep)
    {
        CheckStepButtons();
    }

    /// <summary>
    /// Activate next action.
    /// </summary>
    public void NextAction()
    {
        var actionList = ActivityManager.Instance.ActionsOfTypeAction;

        //return if there is no next action
        if (actionList.IndexOf(ActivityManager.Instance.ActiveAction) >= actionList.Count - 1) return;

        if (ActivityManager.Instance.ActiveAction != null)
        {
            ActivityManager.Instance.ActiveAction.isCompleted = true;
        }

        ActivityManager.Instance.ActivateNextAction();

        TaskStationDetailMenu.Instance.SelectedButton = null;

        CheckStepButtons();
    }

    /// <summary>
    /// Force activate previous action.
    /// </summary>
    public void PreviousAction()
    {
        ActivityManager.Instance.ActivatePreviousAction();

        CheckStepButtons();
    }

    public void NextPage()
    {
        Page++;
    }

    public void PreviousPage()
    {
        Page--;
    }

    public async void AddAction()
    {
        await ActivityManager.Instance.AddAction(Vector3.zero);
    }

    private void OnActionCreated(Action action)
    {
        UpdateUI();
    }

    private void OnActionChanged(Action action)
    {
        UpdateUI();
    }

    private void OnActionDeleted(string actionId)
    {
        ActivityManager.Instance.ActivateNextAction();
        UpdateUI();
    }

}

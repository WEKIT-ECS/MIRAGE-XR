using LearningExperienceEngine;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionListMenu : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    [SerializeField] private GameObject actionItemPrefab;
    [SerializeField] private RectTransform listViewParent;
    [SerializeField] private Button previousStepButton;
    [SerializeField] private Button nextStepButton;
    [SerializeField] private Text pageLabel;
    [SerializeField] private InputField titleText;
    public InputField TitleText => titleText;

    [SerializeField] private GameObject addActionStepButton;
    [SerializeField] private GameObject restartActivityPrompt;
    public GameObject AddActionStepButton => addActionStepButton;

    [HideInInspector]
    public GameObject uploadProgressText { get; private set; }

    public static ActionListMenu Instance { get; private set; }

    private int page;
    private int totalNumberOfPages;
    private const int itemsPerPage = 10;
    public ActionListItem[] ActionListItems { get; } = new ActionListItem[itemsPerPage];

    private int Page
    {
        get => page;
        set => page = Mathf.Clamp(value, 0, totalNumberOfPages - 1);
    }

    private void Start()
    {
        if (!Instance)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        if (!RootObject.Instance.PlatformManager.WorldSpaceUi)
        {
            GetComponent<Canvas>().enabled = false;
            return;
        }

        // hide the upload progress number
        if (uploadProgressText == transform.FindDeepChild("UploadProgress").gameObject)
            uploadProgressText.SetActive(false);


        for (int i = 0; i < itemsPerPage; i++)
        {
            GameObject itemInstance = Instantiate(actionItemPrefab, listViewParent);
            ActionListItems[i] = itemInstance.GetComponent<ActionListItem>();
        }



        if (activityManager.IsReady)
        {
            Init();

            // activate the first step
            //activityManager.ActivateByID(activityManager.ActionsOfTypeAction[0].id);
        }

        if (activityManager.Activity.start != null)
        {
            if (activityManager.ActiveAction.id != activityManager.Activity.start)
            {
                GameObject.Instantiate(restartActivityPrompt, transform.position + transform.forward * 0.5f, transform.rotation);
            }
        }

        Debug.LogTrace("Action list menu start called");
        LearningExperienceEngine.EventManager.OnInitUi += Init;
        LearningExperienceEngine.EventManager.OnActivateAction += OnActivateAction;
        LearningExperienceEngine.EventManager.OnDeactivateAction += OnDeactivateAction;
        LearningExperienceEngine.EventManager.OnActionCreated += OnActionCreated;
        LearningExperienceEngine.EventManager.OnActionDeleted += OnActionDeleted;
        LearningExperienceEngine.EventManager.OnActionModified += OnActionChanged;
        MirageXR.EventManager.OnNextByVoice += NextAction;
        MirageXR.EventManager.OnBackByVoice += PreviousAction;
        titleText.onValueChanged.AddListener(delegate { LearningExperienceEngine.EventManager.NotifyOnActivityRenamed(); });
    }

    private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnInitUi -= Init;
        LearningExperienceEngine.EventManager.OnActivateAction -= OnActivateAction;
        LearningExperienceEngine.EventManager.OnDeactivateAction -= OnDeactivateAction;
        LearningExperienceEngine.EventManager.OnActionCreated -= OnActionCreated;
        LearningExperienceEngine.EventManager.OnActionDeleted -= OnActionDeleted;
        LearningExperienceEngine.EventManager.OnActionModified -= OnActionChanged;
        MirageXR.EventManager.OnNextByVoice -= NextAction;
        MirageXR.EventManager.OnBackByVoice -= PreviousAction;
        titleText.onValueChanged.RemoveListener(delegate { LearningExperienceEngine.EventManager.NotifyOnActivityRenamed(); });
    }

    private void Init()
    {
        page = 0;
        titleText.text = activityManager.Activity.name;

        UpdateUI();
        addActionStepButton = gameObject.transform.Find("Panel").Find("ButtonAdd").gameObject;
    }


    private void UpdateUI()
    {
        totalNumberOfPages = (activityManager.ActionsOfTypeAction.Count - 1) / itemsPerPage > 0 ? itemsPerPage : 1;
        DisplayList();
        CheckStepButtons();
        UpdatePageLabel();
    }

    private void DisplayList()
    {
        int startIndexOnPage = itemsPerPage * page;
        int itemCount = Mathf.Min(itemsPerPage, activityManager.ActionsOfTypeAction.Count - startIndexOnPage);

        for (int i = 0; i < itemCount; i++)
        {
            int index = startIndexOnPage + i;
            ActionListItems[i].Content = activityManager.ActionsOfTypeAction[index];
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
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        int activeIndex = activityManager.ActionsOfTypeAction.IndexOf(activityManager.ActiveAction);
        previousStepButton.interactable = activeIndex > 0;

        bool isLastStep = activeIndex >= activityManager.ActionsOfTypeAction.Count - 1;

        if (isLastStep)
        {
            activityManager.ActiveAction.isCompleted = true;
        }

        bool isCompleted = activityManager.ActiveAction == null || activityManager.ActiveAction.isCompleted;
        nextStepButton.interactable = (!isLastStep || !isCompleted) && activeIndex < activityManager.ActionsOfTypeAction.Count - 1;

        // automatically go to next page if step is on next page
        if (activeIndex + 1 > itemsPerPage * (page + 1))
        {
            NextPage();
        }
        // automatically go to previous page if step is on previous page
        else if (activeIndex + 1 <= itemsPerPage * page)
        {
            PreviousPage();
        }

        UpdatePageLabel();

        StartCoroutine(ButtonShortTimeDeactivation(previousStepButton.interactable, nextStepButton.interactable));

        // if the last step is done, next time start again from the first step
        if (isLastStep && isCompleted)
        {
            PlayerPrefs.SetString(activityManager.Activity.id, "StartingAction");
            PlayerPrefs.Save();
        }

        // find the target annotation of this action
        FindNavigatorTarget();
    }

    private IEnumerator ButtonShortTimeDeactivation(bool previousButtonIsActive, bool nextButtonIsActive)
    {
        previousStepButton.interactable = false;
        nextStepButton.interactable = false;
        yield return new WaitForSeconds(0.5f);
        previousStepButton.interactable = previousButtonIsActive;
        nextStepButton.interactable = nextButtonIsActive;
    }

    private void UpdatePageLabel()
    {
        int displayPageIndex = activityManager.ActionsOfTypeAction.IndexOf(activityManager.ActiveAction) + 1;
        int totalNumberOfAction = activityManager.ActionsOfTypeAction.Count;
        pageLabel.text = $"{displayPageIndex:00}/{totalNumberOfAction:00}";
    }

    private void FindNavigatorTarget()
    {
        var targetAnnotation = activityManager.ActiveAction.enter.activates.Find(a => a.state == "target");
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

    public static Transform CorrectTargetObject(LearningExperienceEngine.ToggleObject annotation)
    {
        try
        {
            Transform target = GameObject.Find(annotation.poi).transform;

            switch (annotation.predicate)
            {

                case string type when type.StartsWith("char"):
                    target = target.GetComponentInChildren<MirageXR.CharacterController>().transform;
                    break;
                case string type when type.StartsWith("pick"):
                    target = target.GetComponentInChildren<PickAndPlaceController>().PickObject;
                    break;
                case "ghosttracks":
                    target = target.GetComponentInChildren<GhostRecordPlayer>().transform;
                    break;
                default:
                    //for all other augmentations the model will be the first child of the poi
                    target = target.GetChild(0);
                    break;
            }
            return target;
        }
        catch (Exception e)
        {
            Debug.LogError("Could not find model for augmentation " + annotation.predicate + " at POI: " + annotation.poi + "\nException: " + e);
            return null;
        }
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
        var actionList = activityManager.ActionsOfTypeAction;

        // return if there is no next action
        if (actionList.IndexOf(activityManager.ActiveAction) >= actionList.Count - 1) return;

        if (activityManager.ActiveAction != null)
        {
            activityManager.ActiveAction.isCompleted = true;
        }

        activityManager.ActivateNextAction();

        TaskStationDetailMenu.Instance.SelectedButton = null;

        CheckStepButtons();
    }

    /// <summary>
    /// Force activate previous action.
    /// </summary>
    public void PreviousAction()
    {
        activityManager.ActivatePreviousAction();

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
        await activityManager.AddAction(Vector3.zero);
    }

    private void OnActionCreated(LearningExperienceEngine.Action action)
    {
        UpdateUI();
    }

    private void OnActionChanged(LearningExperienceEngine.Action action)
    {
        UpdateUI();
    }

    private void OnActionDeleted(string actionId)
    {
        activityManager.ActivateNextAction();
        UpdateUI();
    }
}
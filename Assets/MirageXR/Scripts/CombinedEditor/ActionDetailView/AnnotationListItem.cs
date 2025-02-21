using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationListItem : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    [SerializeField] private Text textField;
    [SerializeField] private InputField startStepInput;
    [SerializeField] private InputField endStepInput;
    [SerializeField] private GameObject targetIcon;
    [SerializeField] private GameObject lockIcon;

    private Button button;

    public ActionDetailView ParentView { get; private set; }

    public LearningExperienceEngine.ToggleObject DisplayedAnnotation { get; private set; }

    public delegate void OnAnnotationItemClickedDelegate(LearningExperienceEngine.ToggleObject annotation);

    public event OnAnnotationItemClickedDelegate OnAnnotationItemClicked;

    public void TargetIconVisibility(bool visibility)
    {
        targetIcon.SetActive(visibility);
    }


    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        OnEditModeChanged(activityManager.EditModeActive);

        startStepInput.onEndEdit.AddListener(delegate { AdjustPoiLifetime(); });
        endStepInput.onEndEdit.AddListener(delegate { AdjustPoiLifetime(); });

        targetIcon.GetComponent<Button>().onClick.AddListener(RemoveMyTarget);
    }

    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;

        OnEditModeChanged(activityManager.EditModeActive);
    }

    private void OnDisable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool editModeActive)
    {
        button.interactable = editModeActive;
        targetIcon.GetComponent<Button>().interactable = editModeActive;
        if (DisplayedAnnotation != null)
        {
            if (SetLockActive())
            {
                lockIcon.SetActive(editModeActive);
            }
        }
    }

    public void SetUp(ActionDetailView parentView, LearningExperienceEngine.ToggleObject annotation)
    {
        ParentView = parentView;
        DisplayedAnnotation = annotation;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (ParentView == null || DisplayedAnnotation == null)
        {
            return;
        }
        textField.text = DisplayedAnnotation.predicate;

        var actionList = activityManager.Activity.actions;
        var startStep = actionList.IndexOf(actionList.Find(a => a.enter.activates.Find(p => p.poi == DisplayedAnnotation.poi) != null));
        var lastStep = actionList.FindLastIndex(a => a.enter.activates.Find(p => p.poi == DisplayedAnnotation.poi) != null);
        startStepInput.text = (startStep + 1).ToString();
        endStepInput.text = (lastStep + 1).ToString();

        targetIcon.SetActive(DisplayedAnnotation.state == "target");
        lockIcon.SetActive(SetLockActive());
    }


    private void RemoveMyTarget()
    {
        activityManager.ActiveAction.enter.activates.ForEach(a => { if (a == DisplayedAnnotation) a.state = ""; });
        activityManager.ActiveAction.exit.deactivates.ForEach(a => { if (a == DisplayedAnnotation) a.state = ""; });
        TaskStationDetailMenu.Instance.NavigatorTarget = null;
        targetIcon.SetActive(false);
    }

    private void AdjustPoiLifetime()
    {
        var actionList = activityManager.ActionsOfTypeAction;

        var startIndex = int.Parse(startStepInput.text) > 0 ? int.Parse(startStepInput.text) - 1 : 0;
        var endIndex = int.Parse(endStepInput.text) <= actionList.Count ? int.Parse(endStepInput.text) - 1 : actionList.Count - 1;

        if (startIndex == 0)
        {
            startStepInput.text = "1";
        }

        if (endIndex > actionList.Count - 1)
        {
            endStepInput.text = actionList.Count.ToString();
            endIndex = actionList.Count - 1;
        }

        if (startIndex > endIndex)
        {
            startIndex = endIndex;
            startStepInput.text = startStepInput.text;
        }

        LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAllAugmentationsBetweenSteps(startIndex, endIndex, DisplayedAnnotation, Vector3.zero);

        // On editing the keep alive of the character in each step, save the data (Can use for other augmentations also if needed
        if (DisplayedAnnotation.predicate.StartsWith("char"))
        {
            activityManager.SaveData();
        }

        UpdateUI();
    }

    public void OnItemClicked()
    {
        OnAnnotationItemClicked?.Invoke(DisplayedAnnotation);
        TaskStationDetailMenu.Instance.SelectedButton = GetComponent<Button>();
    }

    private bool SetLockActive()
    {
        if (!activityManager.EditModeActive)
        {
            return false;
        }

        switch (DisplayedAnnotation.predicate)
        {
            case string a when a.StartsWith("label"):
                return true;
                break;
            case string a when a.StartsWith("effect"):
                return true;
                break;
            case string a when a.StartsWith("act"):
                return true;
                break;
            case string a when a.StartsWith("image"):
                return true;
                break;
            case string a when a.StartsWith("video"):
                return true;
                break;
            case string a when a.StartsWith("3d"):
                return true;
                break;
            default:
                return false;
                break;
        }
    }
}

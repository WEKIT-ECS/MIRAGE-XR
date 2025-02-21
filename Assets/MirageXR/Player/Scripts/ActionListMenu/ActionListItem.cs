using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class ActionListItem : MonoBehaviour
{
    private static LearningExperienceEngine.BrandManager brandManager => LearningExperienceEngine.LearningExperienceEngine.Instance.BrandManager;
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text captionLabel;
    [SerializeField] private Text numberLabel;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Color standardColor;
    [SerializeField] private Color completedColor;
    [SerializeField] private Image checkIcon;

    public Button DeleteButton => deleteButton;

    public LearningExperienceEngine.Action Content { get; set; }

    public int DataIndex { get; set; }

    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnActivateAction += OnActivateAction;
        LearningExperienceEngine.EventManager.OnEditModeChanged += SetEditModeState;
        if (activityManager != null)
        {
            SetEditModeState(activityManager.EditModeActive);
        }
        UpdateView();
    }

    private void OnDisable()
    {
        LearningExperienceEngine.EventManager.OnActivateAction -= OnActivateAction;
        LearningExperienceEngine.EventManager.OnEditModeChanged -= SetEditModeState;
    }

    private void OnActivateAction(string action)
    {
        UpdateView();
    }

    private void SetEditModeState(bool editModeActive)
    {
        deleteButton.gameObject.SetActive(Content != null && editModeActive);
    }

    public void UpdateView()
    {
        if (Content == null)
        {
            gameObject.name = "Unused Item";
            captionLabel.text = string.Empty;
            numberLabel.text = string.Empty;
            backgroundImage.color = standardColor;
            checkIcon.gameObject.SetActive(false);
        }
        else
        {
            gameObject.name = $"Step-{Content.id}";
            captionLabel.text = Content.instruction.title;
            int displayNumber = DataIndex + 1;
            numberLabel.text = displayNumber.ToString("00");
            bool isActive = Content.id == activityManager.ActiveActionId;

            if (isActive)
            {
                backgroundImage.color = brandManager.DefaultSecondaryColor;
                checkIcon.gameObject.SetActive(false);
            }
            else if (Content.isCompleted)
            {
                backgroundImage.color = completedColor;
                checkIcon.gameObject.SetActive(true);
            }
            else
            {
                backgroundImage.color = standardColor;
                checkIcon.gameObject.SetActive(false);
            }
        }

        // enable/disable this as raycast target for empty or none empty rows
        foreach (var textComponent in gameObject.GetComponentsInChildren<Text>())
        {
            textComponent.raycastTarget = Content != null;
        }

        foreach (var imageComponent in gameObject.GetComponentsInChildren<Image>())
        {
            imageComponent.raycastTarget = Content != null;
        }

        SetEditModeState(activityManager.EditModeActive);
    }

    public void DeleteAction()
    {
        if (activityManager.ActionsOfTypeAction.Count > 1)
        {
            // unchild the task station menu before destroying the TS
            TaskStationDetailMenu.Instance.gameObject.transform.SetParent(null);
            DialogWindow.Instance.Show("Warning!", "Are you sure you want to delete this step?",
                new DialogButtonContent("Yes", () => activityManager.DeleteAction(Content.id)),
                new DialogButtonContent("No"));
        }
    }
}

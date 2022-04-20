using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class ActionListItem : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text captionLabel;
    [SerializeField] private Text numberLabel;
    [SerializeField] private Button deleteButton;

    public Button DeleteButton => deleteButton;

    [SerializeField] private Image checkIcon;

    [SerializeField] private Color standardColor;
    [SerializeField] private Color completedColor;

    public Action Content { get; set; }

    public int DataIndex { get; set; }

    private void OnEnable()
    {
        EventManager.OnActivateAction += OnActivateAction;
        EventManager.OnEditModeChanged += SetEditModeState;
        if (ActivityManager.Instance != null)
        {
            SetEditModeState(ActivityManager.Instance.EditModeActive);
        }
        UpdateView();
    }


    private void OnDisable()
    {
        EventManager.OnActivateAction -= OnActivateAction;
        EventManager.OnEditModeChanged -= SetEditModeState;
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
            captionLabel.text = "";
            numberLabel.text = "";
            backgroundImage.color = standardColor;
            checkIcon.gameObject.SetActive(false);


        }
        else
        {
            gameObject.name = $"Step-{Content.id}";
            captionLabel.text = Content.instruction.title;
            int displayNumber = DataIndex + 1;
            numberLabel.text = displayNumber.ToString("00");
            bool isActive = Content.id == ActivityManager.Instance.ActiveActionId;

            if (isActive)
            {
                backgroundImage.color = BrandManager.Instance.DefaultSecondaryColor;
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


        //enable/disable this as raycast target for empty or none empty rows
        foreach (var textComponent in gameObject.GetComponentsInChildren<Text>())
            textComponent.raycastTarget = Content != null;
        foreach (var imageComponent in gameObject.GetComponentsInChildren<Image>())
            imageComponent.raycastTarget = Content != null;


        SetEditModeState(ActivityManager.Instance.EditModeActive);
    }

    public void DeleteAction()
    {
        if (ActivityManager.Instance.ActionsOfTypeAction.Count > 1)
        {    //unchild the task station menu before destroying the TS
            TaskStationDetailMenu.Instance.gameObject.transform.SetParent(null);
            ActivityManager.Instance.DeleteAction(Content.id);
        }
    }
}

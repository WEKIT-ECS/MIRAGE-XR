using MirageXR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivityEditor : MonoBehaviour
{
    [SerializeField] private Toggle editCheckbox;
    [SerializeField] private InputField activityTitleField;
    [SerializeField] private Button addButton;
    [SerializeField] private Button saveButton;
    public Button GetSaveButton()
    {
        return this.saveButton;
    }
    [SerializeField] private Button uploadButon;
    public Button GetUploadButton()
    {
        return this.uploadButon;
    }
    [SerializeField] private Text loginNeedText;

    [SerializeField] private GameObject updateConfirmPanel;
    [SerializeField] private Text ConfirmPanelText;
    [SerializeField] private Button ConfirmPanelYesButton;
    [SerializeField] private Dropdown optionsDropDown;

    public static ActivityEditor Instance;

    private void Awake()
    {
        EventManager.OnWorkplaceParsed += CheckEditState;
    }

    private void Start()
    {
        if (!Instance)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventManager.OnEditModeChanged += SetEditorState;

        if (activityTitleField.text == string.Empty)
            activityTitleField.text = "New Activity";
        activityTitleField.onValueChanged.AddListener(OnActivityTitleChanged);

        if (ActivityManager.Instance != null)
        {
            SetEditorState(ActivityManager.Instance.EditModeActive);
        }
    }

    private void OnDisable()
    {
        activityTitleField.onValueChanged.RemoveListener(OnActivityTitleChanged);
        EventManager.OnEditModeChanged -= SetEditorState;
    }

    private void OnDestroy()
    {
        EventManager.OnWorkplaceParsed -= CheckEditState;
    }

    private void CheckEditState()
    {
        if (string.IsNullOrEmpty(ActivityManager.Instance.Activity.id))
        {
            ActivityManager.Instance.EditModeActive = true;
        }
        else
        {
            SetEditorState(ActivityManager.Instance.EditModeActive);
        }
        editCheckbox.isOn = ActivityManager.Instance.EditModeActive;
    }

    public void SetEditorState(bool editModeActive)
    {
        activityTitleField.interactable = editModeActive;
        activityTitleField.GetComponent<Image>().enabled = editModeActive;
        addButton.gameObject.SetActive(editModeActive);
        saveButton.gameObject.SetActive(editModeActive);
        uploadButon.gameObject.SetActive(editModeActive);
        loginNeedText.text = string.Empty;
    }

    public void ShowCloneWarningPanel()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData("Clone"),
            new Dropdown.OptionData("Cancel"),
        };

        optionsDropDown.AddOptions(options);
        ConfirmPanelText.text = "You are not the original author of this file! Please select an option:";
        updateConfirmPanel.SetActive(true);
    }

    public void ShowUploadWarningPanel()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData("Update"),
            new Dropdown.OptionData("Clone"),
            new Dropdown.OptionData("Cancel"),
        };

        optionsDropDown.AddOptions(options);
        ConfirmPanelText.text = "This file is exist! Please select an option:";
        updateConfirmPanel.SetActive(true);
    }


    public void DoUploadProcess()
    {
        var option = optionsDropDown.options[optionsDropDown.value].text;

        switch (option)
        {
            case "Update":
                OnUploadButtonClicked(1);
                break;
            case "Clone":
                ActivityManager.Instance.GenerateNewId(true);
                OnUploadButtonClicked(2);
                break;
            case "Cancel":
            default:
                updateConfirmPanel.SetActive(false);
                MoodleManager.Instance.GetProgressText = "Upload";
                optionsDropDown.options.Clear();
                break;
        }
    }


    public void OnEditToggleChanged(bool value)
    {
        Debug.Log("Toggle changed " + value);
        if (ActivityManager.Instance)
        {
            ActivityManager.Instance.EditModeActive = value;
            transform.GetComponentInChildren<Toggle>().isOn = value;
        }
    }

    public void ToggleEditMode()
    {
        bool newValue = !ActivityManager.Instance.EditModeActive;
        OnEditToggleChanged(newValue);
        transform.GetComponentInChildren<Toggle>().isOn = newValue;
    }

    private void OnActivityTitleChanged(string text)
    {
        ActivityManager.Instance.Activity.name = text;
    }

    public void OnSaveButtonClicked()
    {
        EventManager.NotifyOnActivitySaveButtonClicked();
        SaveActivity();
    }

    private void SaveActivity()
    {
        ActivityManager.Instance.SaveData();

        //Reload the activity selection list with the new saved activity
        var sessionListView = Resources.FindObjectsOfTypeAll<SessionListView>()[0];
        if (sessionListView)
            sessionListView.ReloadActivityList();
    }

    public void OpenScreenShot()
    {
        var actionEditor = FindObjectOfType<ActionEditor>();
        var ie = (ImageEditor) actionEditor.CreateEditorView(ContentType.IMAGE);
        var adv = actionEditor.GetDetailView();
        ie.IsThumbnail = true;
        ie.Open(adv.DisplayedAction,null);
    }

    public async void OnUploadButtonClicked(int updateMode)
    {
        EventManager.NotifyOnActivityUploadButtonClicked();

        SaveActivity();

        //clear optionDropDown options if is not empty
        optionsDropDown.options.Clear();

        //hide confirm panel
        updateConfirmPanel.SetActive(false);

        ////Thumbnail is mandatory
        //string thumbnailPath = Path.Combine(ActivityManager.Instance.Path, "thumbnail.jpg");
        //if (!File.Exists(thumbnailPath))
        //{
        //    loginNeedText.text = "Thumbnail not exist!";
        //    return;
        //}else
        //{
        //    loginNeedText.text = "";
        //}

        //login needed for uploading
        if (DBManager.LoggedIn)
        {
            loginNeedText.text = string.Empty;
            await MoodleManager.Instance.UploadFile(ActivityManager.Instance.Path, ActivityManager.Instance.Activity.name, updateMode);
        }
        else
        {
            loginNeedText.text = "Login needed!";
        }
    }
}

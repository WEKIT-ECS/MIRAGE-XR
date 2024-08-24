﻿using LearningExperienceEngine;
using i5.Toolkit.Core.VerboseLogging;
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
    public Button SaveButton => saveButton;

    [SerializeField] private Button uploadButon;
    public Button UploadButton => uploadButon;

    [SerializeField] private Text loginNeedText;

    [SerializeField] private GameObject updateConfirmPanel;
    [SerializeField] private Text ConfirmPanelText;
    [SerializeField] private Button ConfirmPanelYesButton;
    [SerializeField] private Dropdown optionsDropDown;

    public static ActivityEditor Instance { get; private set; }

    private void Awake()
    {
        LearningExperienceEngine.EventManager.OnWorkplaceLoaded += CheckEditState;
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
        MirageXR.EventManager.OnEditModeChanged += SetEditorState;

        if (activityTitleField.text == string.Empty)
            activityTitleField.text = "New Activity";
        activityTitleField.onValueChanged.AddListener(OnActivityTitleChanged);

        if (LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager != null)
        {
            SetEditorState(LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.EditModeActive);
        }
    }

    private void OnDisable()
    {
        activityTitleField.onValueChanged.RemoveListener(OnActivityTitleChanged);
        MirageXR.EventManager.OnEditModeChanged -= SetEditorState;
    }

    private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnWorkplaceLoaded -= CheckEditState;
    }

    private void CheckEditState()
    {
        if (string.IsNullOrEmpty(LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.Activity.id))
        {
            LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.EditModeActive = true;
        }
        else
        {
            SetEditorState(LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.EditModeActive);
        }
        editCheckbox.isOn = LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.EditModeActive;
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
        ConfirmPanelText.text = "Not activity owner. Please choose:";
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
        ConfirmPanelText.text = "Activity already exists on the cloud. Please choose:";
        updateConfirmPanel.SetActive(true);
    }


    public void DoUploadProcess()
    {
        var option = optionsDropDown.options[optionsDropDown.value].text;   //TODO: use optionsDropDown.value instead of string

        switch (option)
        {
            case "Update":
                OnUploadButtonClicked(1);
                break;
            case "Clone":
                LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.CloneActivity();
                OnUploadButtonClicked(2);
                break;
            case "Cancel":
            default:
                updateConfirmPanel.SetActive(false);
                LearningExperienceEngine.LearningExperienceEngine.Instance.moodleManager.GetProgressText = "Upload";
                optionsDropDown.options.Clear();
                break;
        }
    }


    public void OnEditToggleChanged(bool value)
    {
        Debug.LogDebug("Toggle changed " + value);
        if (LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager != null)
        {
            LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.EditModeActive = value;
            transform.GetComponentInChildren<Toggle>().isOn = value;
        }
    }

    public void ToggleEditMode()
    {
        bool newValue = !LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.EditModeActive;
        OnEditToggleChanged(newValue);
        transform.GetComponentInChildren<Toggle>().isOn = newValue;
    }

    private void OnActivityTitleChanged(string text)
    {
        LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.Activity.name = text;
    }

    public void OnSaveButtonClicked()
    {
        MirageXR.EventManager.NotifyOnActivitySaveButtonClicked();
        SaveActivity();
    }

    private void SaveActivity()
    {
        LearningExperienceEngine.EventManager.SaveActivity();
        LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.SaveData();
    }

    public void OpenScreenShot()
    {
        var actionEditor = FindObjectOfType<ActionEditor>();
        var ie = (ImageEditor)actionEditor.CreateEditorView(ContentType.IMAGE);
        var adv = actionEditor.DetailView;
        ie.IsThumbnail = true;
        ie.Open(adv.DisplayedAction, null);
    }

    public async void OnUploadButtonClicked(int updateMode)
    {
        MirageXR.EventManager.NotifyOnActivityUploadButtonClicked();

        SaveActivity();

        // clear optionDropDown options if is not empty
        optionsDropDown.options.Clear();

        // hide confirm panel
        updateConfirmPanel.SetActive(false);

        ////Thumbnail is mandatory
        //string thumbnailPath = Path.Combine(RootModel.Instance.activityManager.Path, "thumbnail.jpg");
        //if (!File.Exists(thumbnailPath))
        //{
        //    loginNeedText.text = "Thumbnail not exist!";
        //    return;
        //}else
        //{
        //    loginNeedText.text = "";
        //}

        // login needed for uploading
        if (LearningExperienceEngine.DBManager.LoggedIn)
        {
            loginNeedText.text = string.Empty;
            await LearningExperienceEngine.LearningExperienceEngine.Instance.moodleManager.UploadFile(LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.ActivityPath, LearningExperienceEngine.LearningExperienceEngine.Instance.activityManager.Activity.name, updateMode);
        }
        else
        {
            loginNeedText.text = "Login needed!";
        }
    }
}

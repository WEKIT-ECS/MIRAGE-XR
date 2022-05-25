using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = MirageXR.Action;

public class StepsListView : BaseView
{
    private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg";
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    private static MoodleManager moodleManager => RootObject.Instance.moodleManager;
    
    [SerializeField] private TMP_InputField _inputFieldName;
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private Toggle _toggleEdit;
    [SerializeField] private Button _btnThumbnail;
    [SerializeField] private Button _btnAddStep;
    [SerializeField] private Button _btnSave;
    [SerializeField] private Button _btnUpload;
    [SerializeField] private Image _imgThumbnail;
    [SerializeField] private Sprite _defaultThumbnail;
    [SerializeField] private StepsListItem _stepsListItemPrefab;
    [SerializeField] private ThumbnailEditorView _thumbnailEditorPrefab;

    private readonly List<StepsListItem> _stepsList = new List<StepsListItem>();
    public RootView rootView => (RootView)_parentView;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        _inputFieldName.onValueChanged.AddListener(OnStepNameChanged);
        _btnAddStep.onClick.AddListener(OnAddStepClick);
        _toggleEdit.onValueChanged.AddListener(OnEditValueChanged);
        _btnSave.onClick.AddListener(OnSaveButtonPressed);
        _btnUpload.onClick.AddListener(OnUploadButtonPressed);
        _btnThumbnail.onClick.AddListener(OnThumbnailButtonPressed);

        EventManager.OnWorkplaceLoaded += OnStartActivity;
        EventManager.OnActionCreated += OnActionCreated;
        EventManager.OnActionDeleted += OnActionDeleted;
        EventManager.OnActionModified += OnActionChanged;
        EventManager.OnEditModeChanged += OnEditModeChanged;
    }

    private void OnDestroy()
    {
        EventManager.OnWorkplaceLoaded -= OnStartActivity;
        EventManager.OnActionCreated -= OnActionCreated;
        EventManager.OnActionDeleted -= OnActionDeleted;
        EventManager.OnActionModified -= OnActionChanged;
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnStartActivity()
    {
        UpdateView();
    }

    private void UpdateView()
    {
        _inputFieldName.text = activityManager.Activity.name;
        var steps = activityManager.ActionsOfTypeAction;
        _stepsList.ForEach(t => t.gameObject.SetActive(false));
        for (var i = 0; i < steps.Count; i++)
        {
            if (_stepsList.Count <= i)
            {
                var obj = Instantiate(_stepsListItemPrefab, _listContent);
                obj.Init(OnStepClick, OnDeleteStepClick);
                _stepsList.Add(obj);
            }
            _stepsList[i].gameObject.SetActive(true);
            _stepsList[i].UpdateView(steps[i], i);
        }
        OnEditModeChanged(activityManager.EditModeActive);
        LoadThumbnail();
    }

    private void LoadThumbnail()
    {
        var path = Path.Combine(activityManager.ActivityPath, THUMBNAIL_FILE_NAME);
        if (!File.Exists(path))
        {
            _imgThumbnail.sprite = _defaultThumbnail;
            return;
        }

        var texture = Utilities.LoadTexture(path);
        var sprite = Utilities.TextureToSprite(texture);
        _imgThumbnail.sprite = sprite;
    }

    private void OnStepNameChanged(string newTitle)
    {
        activityManager.Activity.name = newTitle;
    }

    public void OnDeleteStepClick(Action step)
    {
        if (activityManager.ActionsOfTypeAction.Count > 1)
        {
            DialogWindow.Instance.Show("Warning!", "Are you sure you want to delete this step?",
                new DialogButtonContent("Yes", () => activityManager.DeleteAction(step.id)),
                new DialogButtonContent("No"));
        }
    }

    private void OnStepClick(Action step)
    {
    }

    private void OnAddStepClick()
    {
        AddStep();
    }

    private void OnEditValueChanged(bool value)
    {
        if (activityManager != null)
        {
            activityManager.EditModeActive = value;
        }
    }

    private void OnEditModeChanged(bool value)
    {
        _btnSave.gameObject.SetActive(value);
        _btnUpload.gameObject.SetActive(value);
        _btnAddStep.gameObject.SetActive(value);
        _inputFieldName.interactable = value;
        _btnThumbnail.interactable = value;
        _toggleEdit.isOn = value;

        _stepsList.ForEach(t => t.OnEditModeChanged(value));
    }

    public async void AddStep()
    {
        await activityManager.AddAction(Vector3.zero);
    }

    public void NextStep()
    {
        var activeStep = activityManager.ActiveAction;
        var actionList = activityManager.Activity.actions;
        if (actionList.Last() != activeStep)
        {
            if (activeStep != null) activeStep.isCompleted = true;
            activityManager.ActivateNextAction();
            UpdateView();
        }
    }

    public void PreviousStep()
    {
        var activeStep = activityManager.ActiveAction;
        var actionList = activityManager.Activity.actions;
        if (actionList.First() != activeStep)
        {
            activityManager.ActivatePreviousAction();
            UpdateView();
        }
    }

    private void OnActionCreated(Action action)
    {
        UpdateView();
    }

    private void OnActionChanged(Action action)
    {
        UpdateView();
    }

    private void OnActionDeleted(string actionId)
    {
        activityManager.ActivateNextAction();
        UpdateView();
    }

    public void OnSaveButtonPressed()
    {
        activityManager.SaveData();
        Toast.Instance.Show("Activity saved on your device");
        rootView.activityListView.UpdateListView();
    }

    public void OnUploadButtonPressed()
    {
        if (DBManager.LoggedIn)
        {
            Upload();
        }
        else
        {
            DialogWindow.Instance.Show("You need to log in.", new DialogButtonContent("Ok", null));
        }
    }

    private void OnThumbnailButtonPressed()
    {
        Action<string> onAccept = OnThumbnailAccepted;
        var path = Path.Combine(activityManager.ActivityPath, THUMBNAIL_FILE_NAME);
        PopupsViewer.Instance.Show(_thumbnailEditorPrefab, onAccept, path);
    }

    private void OnThumbnailAccepted(string path)
    {
        LoadThumbnail();
    }

    private async void Upload()
    {
        activityManager.SaveData();
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 0);
        if (response == "Error: File exist, update")
        {
            DialogWindow.Instance.Show("This file is exist! Please select an option:",
                new DialogButtonContent("Update", UploadAndUpdate),
                new DialogButtonContent("Clone", UploadAndCopy),
                new DialogButtonContent("Cancel", null));
            return;
        }

        if (response == "Error: File exist, clone")
        {
            DialogWindow.Instance.Show("You are not the original author of this file! Please select an option:",
                new DialogButtonContent("Clone", UploadAndCopy),
                new DialogButtonContent("Cancel", null));
            return;
        }

        if (result) Toast.Instance.Show("upload completed successfully");
        rootView.activityListView.UpdateListView();
    }

    private async void UploadAndUpdate()
    {
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 1);
        Toast.Instance.Show(result ? "upload completed successfully" : response);
        if (result) rootView.activityListView.UpdateListView();
    }

    private async void UploadAndCopy()
    {
        activityManager.CloneActivity();
        var (result, response) = await moodleManager.UploadFile(activityManager.ActivityPath, activityManager.Activity.name, 2);
        Toast.Instance.Show(result ? "upload completed successfully" : response);
        if (result) rootView.activityListView.UpdateListView();
    }
}

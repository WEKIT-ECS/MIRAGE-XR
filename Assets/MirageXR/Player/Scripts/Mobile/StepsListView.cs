using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = MirageXR.Action;

public class StepsListView : MonoBehaviour
{
    private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg"; 
    
    public static StepsListView Instance { get; private set; }
    
    [SerializeField] private TMP_InputField _inputFieldName;
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private Toggle _toggleEdit;
    [SerializeField] private Button _btnThumbnail;
    [SerializeField] private Button _btnAddStep;
    [SerializeField] private Button _btnSave;
    [SerializeField] private Button _btnUpload;
    [SerializeField] private Image _imgThumbnail;
    [SerializeField] private StepsListItem _stepsListItemPrefab;
    [SerializeField] private ThumbnailEditorView _thumbnailEditorPrefab;

    private readonly List<StepsListItem> _stepsList = new List<StepsListItem>();
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"{Instance.GetType().FullName} must only be a single copy!");
            return;
        }
        
        Instance = this;
    }

    private void Start()
    {
        _inputFieldName.onValueChanged.AddListener(OnStepNameChanged);
        _btnAddStep.onClick.AddListener(OnAddStepClick);
        _toggleEdit.onValueChanged.AddListener(OnEditValueChanged);
        _btnSave.onClick.AddListener(OnSaveButtonPressed);
        _btnUpload.onClick.AddListener(OnUploadButtonPressed);
        _btnThumbnail.onClick.AddListener(OnThumbnailButtonPressed);
        
        EventManager.OnWorkplaceParsed += OnStartActivity;
        EventManager.OnActionCreated += OnActionCreated;
        EventManager.OnActionDeleted += OnActionDeleted;
        EventManager.OnActionModified += OnActionChanged;
        EventManager.OnEditModeChanged += OnEditModeChanged;
    }

    private void OnDestroy()
    {
        EventManager.OnWorkplaceParsed -= OnStartActivity;
        EventManager.OnActionCreated -= OnActionCreated;
        EventManager.OnActionDeleted -= OnActionDeleted;
        EventManager.OnActionModified -= OnActionChanged;
        EventManager.OnEditModeChanged -= OnEditModeChanged;
        Instance = null;
    }

    private void OnStartActivity()
    {
        UpdateView();
    }

    private void UpdateView()
    {
        _inputFieldName.text = ActivityManager.Instance.Activity.name;
        var steps = ActivityManager.Instance.ActionsOfTypeAction;
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
        OnEditModeChanged(ActivityManager.Instance.EditModeActive);
        LoadThumbnail();
    }

    private void LoadThumbnail()
    {
        var path = Path.Combine(ActivityManager.Instance.Path, THUMBNAIL_FILE_NAME);
        if (!File.Exists(path)) return;
        
        var texture = Utilities.LoadTexture(path);
        var sprite = Utilities.TextureToSprite(texture);
        _imgThumbnail.sprite = sprite;
    }
    
    private void OnStepNameChanged(string newTitle)
    {
        ActivityManager.Instance.Activity.name = newTitle;
    }
    
    public void OnDeleteStepClick(Action step)
    {
        if (ActivityManager.Instance.ActionsOfTypeAction.Count > 1)
        {
            ActivityManager.Instance.DeleteAction(step.id);
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
        if (ActivityManager.Instance != null)
        {
            ActivityManager.Instance.EditModeActive = value;
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
        await ActivityManager.Instance.AddAction(Vector3.zero);
    }
    
    public void NextStep()
    {
        var activeStep = ActivityManager.Instance.ActiveAction;
        var actionList = ActivityManager.Instance.Activity.actions;
        if (actionList.Last() != activeStep)
        {
            if (activeStep != null) activeStep.isCompleted = true;
            ActivityManager.Instance.ActivateNextAction();
            UpdateView();
        }
    }
    
    public void PreviousStep()
    {
        var activeStep = ActivityManager.Instance.ActiveAction;
        var actionList = ActivityManager.Instance.Activity.actions;
        if (actionList.First() != activeStep)
        {
            ActivityManager.Instance.ActivatePreviousAction();
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
        ActivityManager.Instance.ActivateNextAction();
        UpdateView();
    }

    public void OnSaveButtonPressed()
    {
        ActivityManager.Instance.SaveData();
        Toast.Instance.Show("Activity saved on your device");
        ActivityListView.Instance.UpdateListView();
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
        var path = Path.Combine(ActivityManager.Instance.Path, THUMBNAIL_FILE_NAME);
        PopupsViewer.Instance.Show(_thumbnailEditorPrefab, onAccept, path);
    }

    private void OnThumbnailAccepted(string path)
    {
        LoadThumbnail();
    }

    private async void Upload()
    {
        ActivityManager.Instance.SaveData();
        var (result, response) = await MoodleManager.Instance.UploadFile(ActivityManager.Instance.Path, ActivityManager.Instance.Activity.name, 0);
        
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
        ActivityListView.Instance.UpdateListView();
    }

    private async void UploadAndUpdate()
    {
        var (result, response) = await MoodleManager.Instance.UploadFile(ActivityManager.Instance.Path, ActivityManager.Instance.Activity.name, 1);
        Toast.Instance.Show(result ? "upload completed successfully" : response);
        if (result) ActivityListView.Instance.UpdateListView();
    }

    private async void UploadAndCopy()
    {
        ActivityManager.Instance.GenerateNewId(true);
        var (result, response) = await MoodleManager.Instance.UploadFile(ActivityManager.Instance.Path, ActivityManager.Instance.Activity.name, 2);
        Toast.Instance.Show(result ? "upload completed successfully" : response);
        if (result) ActivityListView.Instance.UpdateListView();
    }
}

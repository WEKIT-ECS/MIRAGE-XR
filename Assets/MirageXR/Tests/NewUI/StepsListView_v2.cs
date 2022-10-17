using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = MirageXR.Action;

public class StepsListView_v2 : BaseView
{
    private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg";
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    private static MoodleManager moodleManager => RootObject.Instance.moodleManager;

    [SerializeField] private TMP_InputField _inputFieldName;
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private Toggle _toggleEdit;
    [SerializeField] private Button _btnThumbnail;
    [SerializeField] private Button _btnAddStep;
    [SerializeField] private Button _btnNext;
    [SerializeField] private Button _btnPrev;
    [SerializeField] private Image _imgThumbnail;
    [SerializeField] private StepsListItem_v2 _stepsListItemPrefab;
    [SerializeField] private ThumbnailEditorView _thumbnailEditorPrefab;
    [SerializeField] private Sprite _defaultThumbnail;

    private bool edit;

    private readonly List<StepsListItem_v2> _stepsList = new List<StepsListItem_v2>();
    public RootView rootView => (RootView)_parentView;

    public TMP_InputField ActivityNameField => _inputFieldName;
    public Button BtnAddStep => _btnAddStep;

    private void Start()
    {
        UpdateView();
        edit = false;
    }
    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        _inputFieldName.onValueChanged.AddListener(OnStepNameChanged);
        _btnAddStep.onClick.AddListener(OnAddStepClick);
       // _toggleEdit.onValueChanged.AddListener(OnEditValueChanged);
        _btnThumbnail.onClick.AddListener(OnThumbnailButtonPressed);

        EventManager.OnWorkplaceLoaded += OnStartActivity;
        EventManager.OnActivityStarted += OnStartActivity;
        EventManager.NewActivityCreationButtonPressed += OnStartActivity;
        EventManager.OnActionCreated += OnActionCreated;
        EventManager.OnActionDeleted += OnActionDeleted;
        EventManager.OnActionModified += OnActionChanged;
        EventManager.OnEditModeChanged += OnEditModeChanged;

        UpdateView();
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

    public void UpdateView()
    {
        if (activityManager.Activity != null)
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
        else 
        {
            _inputFieldName.text = "";
        }
       
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
        EventManager.NotifyOnActivityRenamed();
    }

    public void OnDeleteStepClick(Action step)
    {
        if (activityManager.ActionsOfTypeAction.Count > 1)
        {
            RootView_v2.Instance.dialog.ShowMiddle("Warning!", "Are you sure you want to delete this step?", "Yes", () => OnActionDeleted(step.id), "No", null);
        }
    }

    private void OnStepClick(Action step)
    {
        activityManager.ActivateActionByID(step.id);
    }

    private void OnAddStepClick()
    {
        AddStep();
        UpdateView();
    }

    public void OnEditValueChanged(bool value)
    {
        if (activityManager != null)
        {
            edit = !edit;
            activityManager.EditModeActive = edit;
        }

        UpdateView();
    }

    private void OnEditModeChanged(bool value)
    {
        _btnAddStep.transform.parent.gameObject.SetActive(value);
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
        if (actionId == activityManager.ActiveAction.id)
        {
            activityManager.ActivateNextAction();
        }

        activityManager.DeleteAction(actionId);
        UpdateView();
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

}

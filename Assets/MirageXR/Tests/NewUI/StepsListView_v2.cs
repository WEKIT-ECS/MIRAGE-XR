﻿using System;
using System.Collections.Generic;
using System.IO;
using MirageXR;
using TiltBrush;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = MirageXR.Action;

public class StepsListView_v2 : BaseView
{
    private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg";

    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [SerializeField] private TMP_Text _textActivityName;
    [SerializeField] private TMP_InputField _inputFieldActivityName;
    [SerializeField] private TMP_InputField _inputFieldActivityDescription;
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private Button _btnAddStep;
    [SerializeField] private Button _btnBack;
    [SerializeField] private Button _btnSettings;
    [SerializeField] private Button _btnThumbnail;
    [SerializeField] private Image _imgThumbnail;
    [SerializeField] private GameObject _defaultThumbnail;
    [SerializeField] private Toggle _toggleSteps;
    [SerializeField] private Toggle _toggleInfo;
    [SerializeField] private Toggle _toggleCalibration;
    [SerializeField] private GameObject _steps;
    [SerializeField] private GameObject _info;
    [SerializeField] private GameObject _calibration;
    [SerializeField] private SettingsView_v2 _settingsViewPrefab;
    [SerializeField] private StepsListItem_v2 _stepsListItemPrefab;
    [SerializeField] private ThumbnailEditorView _thumbnailEditorPrefab;

    private readonly List<StepsListItem_v2> _stepsList = new List<StepsListItem_v2>();

    private ActivityView_v2 _activityView => (ActivityView_v2)_parentView;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        _inputFieldActivityName.onValueChanged.AddListener(OnActivityNameChanged);
        _inputFieldActivityDescription.onValueChanged.AddListener(OnActivityDescriptionChanged);

        _btnAddStep.onClick.AddListener(OnAddStepClick);
        _btnThumbnail.onClick.AddListener(OnThumbnailButtonPressed);

        _toggleSteps.onValueChanged.AddListener(OnToggleStepValueChanged);
        _toggleInfo.onValueChanged.AddListener(OnToggleInfoValueChanged);
        _toggleCalibration.onValueChanged.AddListener(OnToggleCalibrationValueChanged);

        _btnBack.onClick.AddListener(OnBackPressed);
        _btnSettings.onClick.AddListener(OnSettingsPressed);

        EventManager.OnActivityStarted += OnActivityStarted;
        EventManager.OnWorkplaceLoaded += OnStartActivity;
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

    private void UpdateView()
    {
        if (activityManager.Activity != null)
        {
            _textActivityName.text = activityManager.Activity.name;
            _inputFieldActivityName.text = activityManager.Activity.name;
            _inputFieldActivityDescription.text = activityManager.Activity.description;

            var steps = activityManager.ActionsOfTypeAction;
            _stepsList.ForEach(t => t.gameObject.SetActive(false));
            for (var i = 0; i < steps.Count; i++)
            {
                if (_stepsList.Count <= i)
                {
                    var obj = Instantiate(_stepsListItemPrefab, _listContent);
                    obj.Init(OnStepClick, OnStepEditClick, OnDeleteStepClick);
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
            _textActivityName.text = string.Empty;
            _inputFieldActivityName.text = string.Empty;
            _inputFieldActivityDescription.text = string.Empty;
        }
    }

    private void OnBackPressed()
    {
        _activityView.OnBackToHomePressed();
        _steps.SetActive(true);
        _info.SetActive(false);
        _calibration.SetActive(false);
    }

    private void OnSettingsPressed()
    {
        _settingsViewPrefab.Show();
    }

    private void OnToggleStepValueChanged(bool value)
    {
        _steps.SetActive(value);
        _info.SetActive(!value);
        _calibration.SetActive(!value);
        if (value)
        {
            EventManager.NotifyMobilePageNumberChanged(RootView_v2.HelpPage.ActivitySteps);
        }
    }

    private void OnToggleInfoValueChanged(bool value)
    {
        _steps.SetActive(!value);
        _info.SetActive(value);
        _calibration.SetActive(!value);
        if (value)
        {
            EventManager.NotifyMobilePageNumberChanged(RootView_v2.HelpPage.ActivityInfo);
        }
    }

    private void OnToggleCalibrationValueChanged(bool value)
    {
        _steps.SetActive(!value);
        _info.SetActive(!value);
        _calibration.SetActive(value);
        if (value)
        {
            EventManager.NotifyMobilePageNumberChanged(RootView_v2.HelpPage.ActivityCalibration);
        }
    }

    private void LoadThumbnail()
    {
        var path = Path.Combine(activityManager.ActivityPath, THUMBNAIL_FILE_NAME);
        if (!File.Exists(path))
        {
            _defaultThumbnail.SetActive(true);
            _imgThumbnail.gameObject.SetActive(false);
            return;
        }

        var texture = Utilities.LoadTexture(path);
        var sprite = Utilities.TextureToSprite(texture);
        _defaultThumbnail.SetActive(false);
        _imgThumbnail.gameObject.SetActive(true);
        _imgThumbnail.sprite = sprite;
    }

    public void OnDeleteStepClick(Action step, System.Action deleteCallback = null)
    {
        if (activityManager.ActionsOfTypeAction.Count > 1)
        {
            RootView_v2.Instance.dialog.ShowMiddle("Warning!", "Are you sure you want to delete this step?",
                "Yes", () =>
                {
                    activityManager.DeleteAction(step.id);
                    deleteCallback?.Invoke();
                }, "No", null);
        }
    }

    private void OnStepClick(Action step)
    {
        activityManager.ActivateActionByID(step.id).AsAsyncVoid();
    }

    private async void OnStepEditClick(Action step)
    {
        await activityManager.ActivateActionByID(step.id);
        _activityView.ShowStepContent();
    }

    private void OnAddStepClick()
    {
        AddStep();
        UpdateView();
    }

    private void OnEditModeChanged(bool value)
    {
        _btnAddStep.transform.parent.gameObject.SetActive(value);
        _btnThumbnail.interactable = value;

        _stepsList.ForEach(t => t.OnEditModeChanged(value));
    }

    private async void AddStep()
    {
        await activityManager.AddAction(Vector3.zero);
    }

    private void OnActionCreated(Action action)
    {
        UpdateView();
    }

    private void OnActionChanged(Action action)
    {
        UpdateView();
    }

    private void OnActivityStarted()
    {
        UpdateView();
    }

    private void OnActionDeleted(string actionId)
    {
        //activityManager.ActivateNextAction();
        UpdateView();
    }

    private void OnThumbnailButtonPressed()
    {
        RootView_v2.Instance.dialog.ShowMiddleMultiline(
            "Add Image",
            ("Camera", OpenCamera, false),
            ("Gallery", OpenGallery, false),
            ("Cancel", null, true));
    }

    private void OpenCamera()
    {
        Action<string> onAccept = OnThumbnailAccepted;
        var path = Path.Combine(activityManager.ActivityPath, THUMBNAIL_FILE_NAME);
        PopupsViewer.Instance.Show(_thumbnailEditorPrefab, onAccept, path);
    }

    private void OpenGallery()
    {
        //not implemented
    }

    private void OnThumbnailAccepted(string path)
    {
        LoadThumbnail();
    }

    private void OnActivityNameChanged(string title)
    {
        activityManager.Activity.name = title;
        _textActivityName.text = title;
    }

    private void OnActivityDescriptionChanged(string description)
    {
        activityManager.Activity.description = description;
    }
}

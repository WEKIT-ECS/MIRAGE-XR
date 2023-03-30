﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Content = MirageXR.Action;

public class StepsListView_v2 : BaseView
{
    private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg";
    private const int MAX_PICTURE_SIZE = 1024;

    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [Space]
    [SerializeField] private RectTransform _listVerticalContent;
    [SerializeField] private RectTransform _listHorizontalContent;
    [Space]
    [SerializeField] private TMP_Text _textActivityName;
    [SerializeField] private TMP_InputField _inputFieldActivityName;
    [SerializeField] private TMP_InputField _inputFieldActivityDescription;
    [SerializeField] private RectTransform _addStep;
    [SerializeField] private Button _btnAddStep;
    [SerializeField] private Button _btnBack;
    [SerializeField] private Button _btnSettings;
    [SerializeField] private Button _btnThumbnail;
    [SerializeField] private Button _btnCalibration;
    [SerializeField] private Button _btnRecalibration;
    [SerializeField] private Image _imgThumbnail;
    [SerializeField] private GameObject _defaultThumbnail;
    [SerializeField] private Toggle _toggleSteps;
    [SerializeField] private Toggle _toggleInfo;
    [SerializeField] private Toggle _toggleCalibration;
    [SerializeField] private GameObject _steps;
    [SerializeField] private GameObject _info;
    [SerializeField] private GameObject _calibration;
    [SerializeField] private GameObject _first;
    [SerializeField] private GameObject _second;
    [SerializeField] private CalibrationView _calibrationViewPrefab;
    [SerializeField] private ActivitySettings _settingsViewPrefab;
    [SerializeField] private StepsListItem_v2 _stepsListItemPrefab;
    [SerializeField] private ThumbnailEditorView _thumbnailEditorPrefab;

    private readonly List<StepsListItem_v2> _stepsList = new List<StepsListItem_v2>();

    private ActivityView_v2 _activityView => (ActivityView_v2)_parentView;

    private bool _isEditMode;

    [Header("MirageXR calibration pdf file:")]
    public TextAsset calibrationImage;
    private static string calibrationImageFileName = "MirageXR_calibration_image_pdf.pdf";

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        _inputFieldActivityName.onEndEdit.AddListener(OnActivityNameEndEdit);
        _inputFieldActivityDescription.onEndEdit.AddListener(OnActivityDescriptionEndEdit);

        _btnAddStep.onClick.AddListener(OnAddStepClick);
        _btnThumbnail.onClick.AddListener(OnThumbnailButtonPressed);
        _btnCalibration.onClick.AddListener(OnCalibrationPressed);
        _btnRecalibration.onClick.AddListener(OnCalibrationPressed);

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
        EventManager.OnWorkplaceCalibrated += OnWorkplaceCalibrated;
        EventManager.OnActivateAction += OnActionActivated;

        UpdateView();
    }

    private void OnDestroy()
    {
        EventManager.OnActivityStarted -= OnActivityStarted;
        EventManager.OnWorkplaceLoaded -= OnStartActivity;
        EventManager.OnActionCreated -= OnActionCreated;
        EventManager.OnActionDeleted -= OnActionDeleted;
        EventManager.OnActionModified -= OnActionChanged;
        EventManager.OnEditModeChanged -= OnEditModeChanged;
        EventManager.OnWorkplaceCalibrated -= OnWorkplaceCalibrated;
        EventManager.OnActivateAction -= OnActionActivated;
    }

    private void OnStartActivity()
    {
        UpdateView();
    }

    public void UpdateView()
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
                    var obj = Instantiate(_stepsListItemPrefab, _listVerticalContent);
                    obj.Init(OnStepClick, OnStepEditClick, OnDeleteStepClick, OnSiblingIndexChanged);
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

    private void OnActionActivated(string stepId)
    {
        _stepsList.ForEach(t => t.UpdateView());
    }

    private void OnBackPressed()
    {
        _activityView.OnBackToHomePressed();

        _toggleSteps.isOn = true;
    }

    private void OnSettingsPressed()
    {
        PopupsViewer.Instance.Show(_settingsViewPrefab, _activityView.container);
    }

    private void OnToggleStepValueChanged(bool value)
    {
        _steps.SetActive(value);
        _info.SetActive(!value);
        _calibration.SetActive(!value);
        if (value)
        {
            EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActivitySteps);
        }
    }

    private void OnToggleInfoValueChanged(bool value)
    {
        _steps.SetActive(!value);
        _info.SetActive(value);
        _calibration.SetActive(!value);
        if (value)
        {
            EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActivityInfo);
        }
    }

    private void OnToggleCalibrationValueChanged(bool value)
    {
        _steps.SetActive(!value);
        _info.SetActive(!value);
        _calibration.SetActive(value);
        if (value)
        {
            EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActivityCalibration);
        }
    }

    public void SetCalibrationToggle(bool value)
    {
        _toggleCalibration.isOn = value;
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

    public void OnDeleteStepClick(Content step, System.Action deleteCallback = null)
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

    private void OnSiblingIndexChanged(Content step, int oldIndex, int newIndex)
    {
        /*var item1 = _listContent.GetChild(oldIndex).GetComponent<StepsListItem_v2>();

        if (item1)
        {
            activityManager.SwapActions(step, item1.step);
        }
        */
    }

    private void OnStepClick(Content step)
    {
        activityManager.ActivateActionByID(step.id).AsAsyncVoid();
    }

    private async void OnStepEditClick(Content step)
    {
        await activityManager.ActivateActionByID(step.id);
        _activityView.ShowStepContent();
    }

    private void OnAddStepClick()
    {
        OnAddStepClickAsync().AsAsyncVoid();
    }

    private async Task OnAddStepClickAsync()
    {
        var index = _addStep.GetSiblingIndex();
        if (index == 0)
        {
            await activityManager.AddActionToBegin(Vector3.zero);
        }
        else
        {
            var child = _listVerticalContent.GetChild(index - 1);
            var stepListItem = child.GetComponent<StepsListItem_v2>();

            if (stepListItem)
            {
                await activityManager.ActivateActionByID(stepListItem.step.id);
                await activityManager.AddAction(Vector3.zero);
            }
        }

        UpdateView();
        _addStep.SetSiblingIndex(_addStep.GetSiblingIndex() + 1);
        _activityView.ShowStepContent();
    }

    private void OnEditModeChanged(bool value)
    {
        _isEditMode = value;
        _btnAddStep.transform.parent.gameObject.SetActive(value);
        _btnThumbnail.interactable = value;

        _stepsList.ForEach(t => t.OnEditModeChanged(value));
    }

    private void OnWorkplaceCalibrated()
    {
        _first.SetActive(false);
        _second.SetActive(true);
    }

    private void OnActionCreated(Content action)
    {
        UpdateView();
    }

    private void OnActionChanged(Content action)
    {
        UpdateView();
    }

    private void OnActivityStarted()
    {
        _first.SetActive(true);
        _second.SetActive(false);
        UpdateView();
        _addStep.SetAsLastSibling();
    }

    private void OnActionDeleted(string actionId)
    {
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
        PickImage(MAX_PICTURE_SIZE);
    }

    private void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image
                Texture2D _texture2D = NativeGallery.LoadImageAtPath(path, maxSize, false);

                if (_texture2D == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                // Set picture
                var sprite = Utilities.TextureToSprite(_texture2D);
                _defaultThumbnail.SetActive(false);
                _imgThumbnail.gameObject.SetActive(true);
                _imgThumbnail.sprite = sprite;

                // Save picture
                Texture2D tempTexture = _imgThumbnail.sprite.texture;
                byte[] bytes = tempTexture.EncodeToJPG();
                var tempPath = Path.Combine(activityManager.ActivityPath, THUMBNAIL_FILE_NAME);
                File.WriteAllBytes(tempPath, bytes);
            }
        });
    }

    private void OnThumbnailAccepted(string path)
    {
        LoadThumbnail();
    }

    private void OnActivityNameEndEdit(string title)
    {
        activityManager.Activity.name = title;
        _textActivityName.text = title;

        activityManager.SaveData();
    }

    private void OnActivityDescriptionEndEdit(string description)
    {
        activityManager.Activity.description = description;

        activityManager.SaveData();
    }

    private void OnCalibrationPressed()
    {
        if (_isEditMode)
        {
            RootView_v2.Instance.dialog.ShowBottomMultiline(null,
                ("Start Calibration", ShowCalibrationView),
                ("New position of the calibration image", ShowNewPositionCalibrationView),
                ("Get Calibration Image", ShareCalibrationImage));
        }
        else
        {
            RootView_v2.Instance.dialog.ShowBottomMultiline(null,
                ("Start Calibration", ShowCalibrationView),
                ("Get Calibration Image", ShareCalibrationImage));
        }
    }

    private void ShareCalibrationImage()
    {
        StartCoroutine(SharePDFFile());
    }

    private IEnumerator SharePDFFile()
    {
        yield return new WaitForEndOfFrame();

        string filePath = Path.Combine(Application.temporaryCachePath, calibrationImageFileName);
        File.WriteAllBytes(filePath, calibrationImage.bytes);
        new NativeShare().AddFile(filePath).Share();
    }

    private void ShowNewPositionCalibrationView()
    {
        PopupsViewer.Instance.Show(_calibrationViewPrefab, (System.Action)OnCalibrationViewOpened, (System.Action)OnCalibrationViewClosed, true);
    }

    private void ShowCalibrationView()
    {
        PopupsViewer.Instance.Show(_calibrationViewPrefab, (System.Action)OnCalibrationViewOpened, (System.Action)OnCalibrationViewClosed, false);
    }

    private void OnCalibrationViewOpened()
    {
        RootView_v2.Instance.HideBaseView();
    }

    private void OnCalibrationViewClosed()
    {
        RootView_v2.Instance.ShowBaseView();
    }

    public void MoveStepsToHorizontalScroll()
    {
        for (var i = 0; i < _stepsList.Count; i++)
        {
            _stepsList[i].transform.parent = _listHorizontalContent;
        }
    }

    public void MoveStepsToVerticalScroll()
    {
        for (var i = 0; i < _stepsList.Count; i++)
        {
            _stepsList[i].transform.parent = _listVerticalContent;
        }
    }
}

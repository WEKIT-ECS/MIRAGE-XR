using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

public class StepsListView_v2 : BaseView
{
    private const string CALIBRATION_IMAGE_FILE_NAME = "MirageXR_calibration_image_pdf.pdf";
    private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg";
    private const int MAX_PICTURE_SIZE = 1024;

    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    private static LearningExperienceEngine.BrandManager brandManager => LearningExperienceEngine.LearningExperienceEngine.Instance.BrandManager;

    [Space]
    [SerializeField] private RectTransform _listVerticalContent;
    [SerializeField] private RectTransform _listHorizontalContent;
    [SerializeField] private float _moveTimeHorizontalScroll = 0.9f;
    [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Space]
    [SerializeField] private TMP_Text _textActivityName;
    [SerializeField] private TMP_InputField _inputFieldActivityName;
    [SerializeField] private TMP_InputField _inputFieldActivityDescription;
    [SerializeField] private RectTransform _addStep;
    [SerializeField] private Button _btnAddStep;
    [SerializeField] private Button _btnBack;
    [SerializeField] private Button _btnSettings;
    [SerializeField] private Button _btnThumbnail;
    [SerializeField] private Button _btnFloorLevel;
    [SerializeField] private Button _btnWithMarker;
    [SerializeField] private Button _btnMarkerLess;
    [SerializeField] private Button _btnShareImageMarker;
    [SerializeField] private Image _imgThumbnail;
    [SerializeField] private GameObject _defaultThumbnail;
    [SerializeField] private Toggle _toggleSteps;
    [SerializeField] private Toggle _toggleInfo;
    [SerializeField] private Toggle _toggleCalibration;
    [SerializeField] private GameObject _steps;
    [SerializeField] private GameObject _info;
    [SerializeField] private GameObject _calibration;
    [SerializeField] private GameObject _statusCalibrated;
    [SerializeField] private GameObject _statusNotCalibrated;
    [SerializeField] private CalibrationView _calibrationViewPrefab;
    [SerializeField] private ActivitySettings _settingsViewPrefab;
    [SerializeField] private StepsListItem_v2 _stepsListItemPrefab;
    [SerializeField] private ThumbnailEditorView _thumbnailEditorPrefab;

    private readonly List<StepsListItem_v2> _stepsList = new List<StepsListItem_v2>();

    private ActivityView_v2 _activityView => (ActivityView_v2)_parentView;

    private int _addStepSiblingIndex = 0;
    private int _currentStepIndex;
    private Guid _currentStepId;
    private Coroutine _coroutine;
    private Activity _activity;
    private ActivityStep _step;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _inputFieldActivityName.onEndEdit.AddListener(OnActivityNameEndEdit);
        _inputFieldActivityDescription.onEndEdit.AddListener(OnActivityDescriptionEndEdit);

        _btnAddStep.onClick.AddListener(OnAddStepClick);
        _btnThumbnail.onClick.AddListener(OnThumbnailButtonPressed);

        _btnFloorLevel.onClick.AddListener(ShowFloorDetectionOnlyView);
        _btnWithMarker.onClick.AddListener(ShowImageTargetCalibrationView);
        _btnMarkerLess.onClick.AddListener(ShowMarkerLessCalibrationView);
        _btnShareImageMarker.onClick.AddListener(ShareCalibrationImage);

        _toggleSteps.onValueChanged.AddListener(OnToggleStepValueChanged);
        _toggleInfo.onValueChanged.AddListener(OnToggleInfoValueChanged);
        _toggleCalibration.onValueChanged.AddListener(OnToggleCalibrationValueChanged);

        _btnBack.onClick.AddListener(OnBackPressed);
        _btnSettings.onClick.AddListener(OnSettingsPressed);

        /*LearningExperienceEngine.EventManager.OnStartActivity += OnActivityStarted;
        LearningExperienceEngine.EventManager.OnWorkplaceLoaded += OnStartActivity;
        LearningExperienceEngine.EventManager.OnActionCreated += OnActionCreated;
        LearningExperienceEngine.EventManager.OnActionDeleted += OnActionDeleted;
        LearningExperienceEngine.EventManager.OnActionModified += OnActionChanged;
        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
        LearningExperienceEngine.EventManager.OnWorkplaceCalibrated += OnWorkplaceCalibrated;
        LearningExperienceEngine.EventManager.OnActivateAction += OnActionActivated;*/

        RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditModeChanged;
        RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityUpdated;
        RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
        RootObject.Instance.CalibrationManager.OnCalibrated += OnCalibrated;

        UpdateView();
    }

    private void OnCalibrated(bool isCalibrated)
    {
        UpdateCalibrationStatus(isCalibrated);
    }

    private void UpdateCalibrationStatus(bool isCalibrated)
    {
        _statusCalibrated.SetActive(isCalibrated);
        _statusNotCalibrated.SetActive(!isCalibrated);
    }

    private void OnDestroy()
    {
        /*LearningExperienceEngine.EventManager.OnStartActivity -= OnActivityStarted;
        LearningExperienceEngine.EventManager.OnWorkplaceLoaded -= OnStartActivity;
        LearningExperienceEngine.EventManager.OnActionCreated -= OnActionCreated;
        LearningExperienceEngine.EventManager.OnActionDeleted -= OnActionDeleted;
        LearningExperienceEngine.EventManager.OnActionModified -= OnActionChanged;
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
        LearningExperienceEngine.EventManager.OnWorkplaceCalibrated -= OnWorkplaceCalibrated;
        LearningExperienceEngine.EventManager.OnActivateAction -= OnActionActivated;*/
    }

    private void OnActivityUpdated(Activity activity)
    {
        _activity = activity;

        UpdateView();
    }

    private void OnStepChanged(ActivityStep step)
    {
        _step = step;

        UpdateView();
    }

    private void UpdateView()
    {
        if (_activity == null)
        {
            return;
        }

        if (_step == null)
        {
            return;
        }

        foreach (var item in _stepsList)
        {
            Destroy(item.gameObject);
        }

        _stepsList.Clear();

        for (var i = 0; i < _activity.Steps.Count; i++)
        {
            var activityStep = _activity.Steps[i];
            var obj = Instantiate(_stepsListItemPrefab, _listVerticalContent);
            obj.Init(OnStepClick, OnStepEditClick, OnDeleteStepClick, OnSiblingIndexChanged);
            obj.UpdateView(activityStep, i);
            obj.OnEditModeChanged(RootObject.Instance.LEE.ActivityManager.IsEditorMode);
            _stepsList.Add(obj);
        }

        _addStep.SetAsLastSibling();
    }

    /*private void OnActionActivated(Guid stepId)
    {
        _currentStepId = stepId;
        _stepsList.ForEach(t => t.UpdateView());

        if (_listHorizontalContent.gameObject.activeInHierarchy)
        {
            StartCoroutine(ShowSelectedItem(stepId));
        }
    }*/

    private IEnumerator ShowSelectedItem(Guid stepId)
    {
        _currentStepIndex = _stepsList.FindIndex(step => step.step.Id == stepId);
        var newPosition = CalculatePositionForPage(_currentStepIndex);
        MoveTo(newPosition);
        yield return null;
    }

    private Vector3 CalculatePositionForPage(int index)
    {
        var anchoredPosition = _listHorizontalContent.anchoredPosition3D;
        var width = _listHorizontalContent.rect.width;
        var x = -((index * width / _stepsList.Count) - (width * _listHorizontalContent.pivot.x));
        return new Vector3(x, anchoredPosition.y, anchoredPosition.z);
    }

    private void MoveTo(Vector3 newPosition)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        _coroutine = StartCoroutine(MoveToEnumerator(_listHorizontalContent, newPosition, _moveTimeHorizontalScroll, _animationCurve));
    }

    private IEnumerator MoveToEnumerator(RectTransform rectTransform, Vector3 endPosition, float time, AnimationCurve curve = null, Action callback = null)
    {
        curve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);

        var startPosition = rectTransform.anchoredPosition;
        var timer = 0.0f;
        while (timer < 1.0f)
        {
            timer = Mathf.Min(1.0f, timer + (Time.deltaTime / time));
            var value = curve.Evaluate(timer);
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, value);

            yield return null;
        }
        callback?.Invoke();
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
            MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActivitySteps);
        }
    }

    private void OnToggleInfoValueChanged(bool value)
    {
        _steps.SetActive(!value);
        _info.SetActive(value);
        _calibration.SetActive(!value);
        if (value)
        {
            MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActivityInfo);
        }
    }

    private void OnToggleCalibrationValueChanged(bool value)
    {
        _steps.SetActive(!value);
        _info.SetActive(!value);
        _calibration.SetActive(value);
        if (value)
        {
            MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActivityCalibration);
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

        var texture = MirageXR.Utilities.LoadTexture(path);
        var sprite = MirageXR.Utilities.TextureToSprite(texture);
        _defaultThumbnail.SetActive(false);
        _imgThumbnail.gameObject.SetActive(true);
        _imgThumbnail.sprite = sprite;
    }

    public void OnDeleteStepClick(ActivityStep step, Action deleteCallback = null)
    {
        if (activityManager.ActionsOfTypeAction.Count > 1)
        {
            RootView_v2.Instance.dialog.ShowMiddle("Warning!", "Are you sure you want to delete this step?",
                "Yes", () =>
                {
                    RootObject.Instance.LEE.StepManager.RemoveStep(step.Id);
                    deleteCallback?.Invoke();
                }, "No", null);
        }
    }

    private void OnSiblingIndexChanged(ActivityStep step, int oldIndex, int newIndex)
    {
        /*var item1 = _listContent.GetChild(oldIndex).GetComponent<StepsListItem_v2>();

        if (item1)
        {
            activityManager.SwapActions(step, item1.step);
        }
        */
    }

    private void OnStepClick(ActivityStep step)
    {
        RootObject.Instance.LEE.StepManager.GoToStep(step.Id);
    }

    private void OnStepEditClick(ActivityStep step)
    {
        RootObject.Instance.LEE.StepManager.GoToStep(step.Id);
        _activityView.ShowStepContent();
    }

    private void OnAddStepClick()
    {
        OnAddStepClickAsync().AsAsyncVoid();
    }

    private async Task OnAddStepClickAsync()
    {
        //var index = _addStep.GetSiblingIndex();
        var baseCamera = RootObject.Instance.BaseCamera;
        var position = (baseCamera.transform.forward * 0.5f) + baseCamera.transform.position;
        var stepManager = RootObject.Instance.LEE.StepManager;
        var step = stepManager.AddStep(new Location { Position = position, Rotation = Vector3.zero, Scale = Vector3.one });
        stepManager.GoToStep(step.Id);
        //if (index == 0)
        //{
        //    await activityManager.AddActionToBegin(Vector3.zero);
        //}
        //else
        //{
        //    var child = _listVerticalContent.GetChild(index - 1);
        //    var stepListItem = child.GetComponent<StepsListItem_v2>();
        //
        //    if (stepListItem)
        //    {
        //        RootObject.Instance.LEE.StepManager.GoToStep(stepListItem.step.Id);
        //        await activityManager.AddAction(Vector3.zero);
        //    }
        //}

        //_addStep.SetSiblingIndex(_addStep.GetSiblingIndex() + 1);
        _activityView.ShowStepContent();
    }

    private void OnEditModeChanged(bool value)
    {
        _btnAddStep.transform.parent.gameObject.SetActive(value);
        _btnThumbnail.interactable = value;
        _inputFieldActivityName.interactable = value;
        _inputFieldActivityDescription.interactable = value;

        _stepsList.ForEach(t => t.OnEditModeChanged(value));
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
                Texture2D texture2D = NativeGallery.LoadImageAtPath(path, maxSize, false);

                if (texture2D == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                // Set picture
                var sprite = MirageXR.Utilities.TextureToSprite(texture2D);
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

    private void ShareCalibrationImage()
    {
        StartCoroutine(SharePDFFile());
    }

    private IEnumerator SharePDFFile()
    {
        yield return new WaitForEndOfFrame();

        var filePath = Path.Combine(Application.temporaryCachePath, CALIBRATION_IMAGE_FILE_NAME);
        File.WriteAllBytes(filePath, brandManager.CalibrationMarkerPdf.bytes);
        new NativeShare().AddFile(filePath).Share();
    }

    private void ShowImageTargetCalibrationView()
    {
        var isEditMode = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManager.IsEditorMode;
        var isCalibration = RootObject.Instance.CalibrationManager.IsCalibrated;
        PopupsViewer.Instance.Show(_calibrationViewPrefab, (Action)OnCalibrationViewOpened, (Action)OnCalibrationViewClosed, isEditMode, isEditMode && !isCalibration, false, false);
    }

    private void ShowMarkerLessCalibrationView()
    {
        var isEditMode = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManager.IsEditorMode;
        var isCalibration = RootObject.Instance.CalibrationManager.IsCalibrated;
        PopupsViewer.Instance.Show(_calibrationViewPrefab, (Action)OnCalibrationViewOpened, (Action)OnCalibrationViewClosed, isEditMode, isEditMode && !isCalibration, false, true);
    }

    private void ShowFloorDetectionOnlyView()
    {
        PopupsViewer.Instance.Show(_calibrationViewPrefab, (Action)OnCalibrationViewOpened, (Action)OnCalibrationViewClosed, false, true, true);
    }

    private void OnCalibrationViewOpened()
    {
        RootView_v2.Instance.HideBaseView();
    }

    private void OnCalibrationViewClosed()
    {
        RootView_v2.Instance.ShowBaseView();
        UpdateView();
    }

    public void MoveStepsToHorizontalScroll()
    {
        _addStepSiblingIndex = _addStep.GetSiblingIndex();

        for (var i = 0; i < _stepsList.Count; i++)
        {
            _stepsList[i].transform.SetParent(_listHorizontalContent);
            _stepsList[i].transform.localPosition = Vector3.zero;
        }

        Canvas.ForceUpdateCanvases();
        var stepsCount = activityManager.ActionsOfTypeAction.Count;
        if (stepsCount != 1)
        {
            StartCoroutine(ShowSelectedItem(_currentStepId));
        }
    }

    public void MoveStepsToVerticalScroll()
    {
        for (var i = 0; i < _stepsList.Count; i++)
        {
            _stepsList[i].transform.SetParent(_listVerticalContent);
            _stepsList[i].transform.localPosition = Vector3.zero;
        }

        _addStep.SetSiblingIndex(_addStepSiblingIndex);
    }
}

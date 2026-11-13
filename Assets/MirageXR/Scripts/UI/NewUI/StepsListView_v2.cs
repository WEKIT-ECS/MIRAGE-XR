using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Action = System.Action;

public class StepsListView_v2 : BaseView
{
    private const string CALIBRATION_IMAGE_FILE_NAME = "MirageXR_calibration_image_pdf.pdf";
    private const int MAX_PICTURE_SIZE = 1024;
    // new IActivityManager 
    private static IActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManager;
    //private static LearningExperienceEngine.IActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManager;
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
    [FormerlySerializedAs("_btnSettings")]
    [SerializeField] private Button _btnCloseActivity;
    [SerializeField] private Button _btnThumbnail;
    [SerializeField] private Button _btnDeleteThumbnail;
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
    [SerializeField] private StepsListItem_v2 _stepsListItemPrefab;
    [SerializeField] private ThumbnailEditorView _thumbnailEditorPrefab;
    [SerializeField] private RawImage _imgQRCode;
    [SerializeField] private GameObject _qrCodeContainer;
    [SerializeField] private MirageXR.DigitalTwinRepositionDialog _digitalTwinRepositionPrefab;

    private readonly List<StepsListItem_v2> _stepsList = new List<StepsListItem_v2>();

    private ActivityView_v2 _activityView => (ActivityView_v2)_parentView;

    private int _addStepSiblingIndex = 0;
    private int _currentStepIndex;
    private Guid _currentStepId;
    private Coroutine _coroutine;
    private Activity _activity;
    private ActivityStep _step;

    private GameObject _panelDigitalTwinSettings;
    private RectTransform _panelDigitalTwinSettingsRect;
    private SwitchToggle _toggleDigitalTwin;
    private Toggle _toggleDigitalTwinInner;
    private TMP_Text _txtDigitalTwinToggleHeader;
    private Button _btnDigitalTwinStyle;
    private TMP_Text _txtDigitalTwinStyleHeader;
    private Button _btnAssignRoomModel;
    private TMP_Text _txtAssignRoomModelLink;
    private Button _btnRepositionDigitalTwin;
    private GameObject _rowRepositionDigitalTwin;
    private GameObject _digitalTwinGalleryOverlay;
    private GameObject _digitalTwinRepositionOverlay;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        if (_inputFieldActivityDescription != null)
        {
            _inputFieldActivityDescription.lineType = TMP_InputField.LineType.MultiLineNewline;
            if (_inputFieldActivityDescription.textComponent != null)
            {
                _inputFieldActivityDescription.textComponent.enableWordWrapping = true;
            }
        }

        _inputFieldActivityName.onEndEdit.AddListener(OnActivityNameEndEdit);
        _inputFieldActivityDescription.onEndEdit.AddListener(OnActivityDescriptionEndEdit);

        _btnAddStep.onClick.AddListener(OnAddStepClick);
        _btnThumbnail.onClick.AddListener(OnThumbnailButtonPressed);
        _btnDeleteThumbnail?.onClick.AddListener(() => DeleteThumbnailAsync().Forget());

        _btnFloorLevel.onClick.AddListener(ShowFloorDetectionOnlyView);
        _btnWithMarker.onClick.AddListener(ShowImageTargetCalibrationView);
        _btnMarkerLess.onClick.AddListener(ShowMarkerLessCalibrationView);
        _btnShareImageMarker.onClick.AddListener(ShareCalibrationImage);
        InitDigitalTwinCalibrationButtons();

        _toggleSteps.onValueChanged.AddListener(OnToggleStepValueChanged);
        _toggleInfo.onValueChanged.AddListener(OnToggleInfoValueChanged);
        _toggleCalibration.onValueChanged.AddListener(OnToggleCalibrationValueChanged);

        _btnBack.onClick.AddListener(OnBackPressed);
        if (_btnCloseActivity != null)
        {
            _btnCloseActivity.onClick.AddListener(OnCloseActivityPressed);
        }

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
        RootObject.Instance.LEE.ActivityManager.OnActivityClosed += OnActivityClosed;
        RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
        RootObject.Instance.CalibrationManager.OnCalibrated += OnCalibrated;
        if (RootObject.Instance?.RoomTwinManager != null)
        {
            RootObject.Instance.RoomTwinManager.OnRoomTwinStateChanged += UpdateDigitalTwinButtonsUI;
        }

        if (_activity == null && activityManager?.Activity != null)
        {
            _activity = activityManager.Activity;
        }

        UpdateView();
        UpdateInfoFields();
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

        if (_imgQRCode != null && _imgQRCode.texture != null)
        {
            Destroy(_imgQRCode.texture);
            _imgQRCode.texture = null;
        }

        if (_digitalTwinGalleryOverlay != null)
        {
            Destroy(_digitalTwinGalleryOverlay);
            _digitalTwinGalleryOverlay = null;
        }

        if (_digitalTwinRepositionOverlay != null)
        {
            Destroy(_digitalTwinRepositionOverlay);
            _digitalTwinRepositionOverlay = null;
        }

        if (RootObject.Instance?.RoomTwinManager != null)
        {
            RootObject.Instance.RoomTwinManager.OnRoomTwinStateChanged -= UpdateDigitalTwinButtonsUI;
        }

        if (RootObject.Instance?.LEE?.ActivityManager != null)
        {
            RootObject.Instance.LEE.ActivityManager.OnActivityClosed -= OnActivityClosed;
        }
    }

    private void OnActivityClosed()
    {
        CloseDigitalTwinRepositionOverlay();
        CloseDigitalTwinGalleryOverlay();
        UpdateDigitalTwinButtonsUI();
    }

    private void OnActivityUpdated(Activity activity)
    {
        _activity = activity;

        UpdateView();
        UpdateInfoFields();
        UpdateQRCode();
        UpdateDigitalTwinButtonsUI();
    }

    private void OnStepChanged(ActivityStep step)
    {
        _step = step;
        _currentStepId = step.Id;

        if (_activity != null && _stepsList.Count == _activity.Steps.Count)
        {
            for (var i = 0; i < _stepsList.Count; i++)
            {
                _stepsList[i].UpdateView(_activity.Steps[i], i);
            }
        }
        else
        {
            UpdateView();
        }

        if (_listHorizontalContent != null && _listHorizontalContent.gameObject.activeInHierarchy)
        {
            StartCoroutine(ShowSelectedItem(step.Id));
        }
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

        var isHorizontal = _listHorizontalContent != null && _listHorizontalContent.gameObject.activeInHierarchy;
        var targetParent = isHorizontal ? _listHorizontalContent : _listVerticalContent;

        for (var i = 0; i < _activity.Steps.Count; i++)
        {
            var activityStep = _activity.Steps[i];
            var obj = Instantiate(_stepsListItemPrefab, targetParent);
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

    public void ScrollToStep(Guid stepId)
    {
        StartCoroutine(ShowSelectedItem(stepId));
    }

    private IEnumerator ShowSelectedItem(Guid stepId)
    {
        _currentStepIndex = _stepsList.FindIndex(step => step.step.Id == stepId);
        if (_currentStepIndex < 0 || _currentStepIndex >= _stepsList.Count)
        {
            yield break;
        }

        Canvas.ForceUpdateCanvases();
        yield return null;

        var newPosition = CalculatePositionForPage(_currentStepIndex);
        MoveTo(newPosition);
    }

    private Vector3 CalculatePositionForPage(int index)
    {
        var anchoredPosition = _listHorizontalContent.anchoredPosition3D;
        if (index < 0 || index >= _stepsList.Count || _listHorizontalContent == null)
        {
            return anchoredPosition;
        }

        var viewport = _listHorizontalContent.parent as RectTransform;
        var viewportWidth = viewport != null ? viewport.rect.width : _listHorizontalContent.rect.width;
        var contentWidth = _listHorizontalContent.rect.width;

        if (contentWidth <= viewportWidth)
        {
            return new Vector3(0f, anchoredPosition.y, anchoredPosition.z);
        }

        var itemRect = _stepsList[index].transform as RectTransform;
        var itemX = itemRect != null ? itemRect.localPosition.x : (index + 0.5f) * (contentWidth / _stepsList.Count);

        // Center item in the viewport and clamp within valid scroll bounds
        var desiredX = (viewportWidth * 0.5f) - itemX;
        var minX = -(contentWidth - viewportWidth);
        var clampedX = Mathf.Clamp(desiredX, minX, 0f);

        return new Vector3(clampedX, anchoredPosition.y, anchoredPosition.z);
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

    private void OnCloseActivityPressed()
    {
        CloseDigitalTwinRepositionOverlay();
        CloseDigitalTwinGalleryOverlay();
        LearningExperienceEngine.EventManager.ClearAll();
        RootObject.Instance?.LEE?.ActivityManager?.CloseActivity();
        _activityView.OnBackToHomePressed();
        _toggleSteps.isOn = true;
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
            UpdateInfoFields();
            UpdateQRCode();
        }
    }

    private void UpdateInfoFields()
    {
        if (_activity != null)
        {
            if (_textActivityName != null)
            {
                _textActivityName.text = _activity.Name;
            }

            if (_inputFieldActivityName != null)
            {
                _inputFieldActivityName.text = _activity.Name;
            }

            if (_inputFieldActivityDescription != null)
            {
                _inputFieldActivityDescription.text = _activity.Description;
            }

            LoadThumbnail();
        }
    }

    private void UpdateQRCode()
    {
        if (_imgQRCode == null)
        {
            return;
        }

        if (_activity != null && _activity.Id != Guid.Empty)
        {
            var texture = QRCodeGeneratorService.GenerateActivityQRCodeTexture(_activity.Id);
            if (texture != null)
            {
                if (_imgQRCode.texture != null)
                {
                    Destroy(_imgQRCode.texture);
                }
                _imgQRCode.texture = texture;
                if (_qrCodeContainer != null)
                {
                    _qrCodeContainer.SetActive(true);
                }
                return;
            }
        }

        if (_qrCodeContainer != null)
        {
            _qrCodeContainer.SetActive(false);
        }
    }

    private void OnToggleCalibrationValueChanged(bool value)
    {
        _steps.SetActive(!value);
        _info.SetActive(!value);
        _calibration.SetActive(value);
        if (value)
        {
            UpdateDigitalTwinButtonsUI();
            MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActivityCalibration);
        }
    }

    public void SetCalibrationToggle(bool value)
    {
        _toggleCalibration.isOn = value;
    }

    /// <summary>
    /// Loads and displays the thumbnail for the current activity.
    /// If the thumbnail file does not exist, it displays a default thumbnail.
    /// </summary>
    /// <remarks>
    /// This method is currently not implement due to a lack of an endpoint on the server side.
    /// At the moment a default thumbnail is shown. See this ticket for more information: 
    /// https://github.com/WEKIT-ECS/MIRAGE-XR/issues/2381
    /// </remarks>
    private void SetButtonDeleteThumbnailActive(bool active)
    {
        if (_btnDeleteThumbnail != null)
        {
            _btnDeleteThumbnail.gameObject.SetActive(active);
            if (active)
            {
                _btnDeleteThumbnail.transform.SetAsLastSibling();
            }
        }
    }

    private void LoadThumbnail()
    {
        LoadThumbnailAsync().Forget();
    }

    private async UniTaskVoid LoadThumbnailAsync()
    {
        if (_activity == null)
        {
            _defaultThumbnail.SetActive(true);
            _imgThumbnail.gameObject.SetActive(false);
            SetButtonDeleteThumbnailActive(false);
            return;
        }

        var hasThumbnail = _activity.Thumbnail != null && _activity.Thumbnail.Id != Guid.Empty;
        if (!hasThumbnail)
        {
            _defaultThumbnail.SetActive(true);
            _imgThumbnail.gameObject.SetActive(false);
            SetButtonDeleteThumbnailActive(false);
            return;
        }

        var fileId = _activity.Thumbnail.Id;
        var texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(_activity.Id, fileId);
        if (texture == null)
        {
            await RootObject.Instance.LEE.MediaManager.DownloadMediaFileAsync(_activity.Id, fileId);
            texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(_activity.Id, fileId);
        }

        if (texture != null)
        {
            var sprite = MirageXR.Utilities.TextureToSprite(texture);
            _defaultThumbnail.SetActive(false);
            _imgThumbnail.gameObject.SetActive(true);
            _imgThumbnail.sprite = sprite;
            SetButtonDeleteThumbnailActive(true);
        }
        else
        {
            _defaultThumbnail.SetActive(true);
            _imgThumbnail.gameObject.SetActive(false);
            SetButtonDeleteThumbnailActive(false);
        }
    }

    private async UniTask SaveThumbnailAsync(Texture2D texture)
    {
        if (_activity == null || texture == null)
        {
            return;
        }

        var sprite = MirageXR.Utilities.TextureToSprite(texture);
        _defaultThumbnail.SetActive(false);
        _imgThumbnail.gameObject.SetActive(true);
        _imgThumbnail.sprite = sprite;
        SetButtonDeleteThumbnailActive(true);

        try
        {
            var activityId = _activity.Id;

            // Delete existing server and local thumbnail if one exists
            if (_activity.Thumbnail != null && _activity.Thumbnail.Id != Guid.Empty)
            {
                await RootObject.Instance.LEE.MediaManager.DeleteMediaFileAsync(activityId, _activity.Thumbnail.Id);
            }

            var fileId = Guid.NewGuid();
            // Save to local media folder
            await RootObject.Instance.LEE.MediaManager.SaveToMediaFileAsync(texture, activityId, fileId);

            // Update activity model metadata
            var file = new FileModel
            {
                Id = fileId,
                CreationDate = DateTime.UtcNow
            };
            await activityManager.SetThumbnailAsync(file);

            // Upload to server asset repository
            await RootObject.Instance.LEE.AssetsManager.UploadMediaFileAsync(activityId, fileId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save thumbnail: {ex}");
        }
    }

    private async UniTask DeleteThumbnailAsync()
    {
        if (_activity == null)
        {
            return;
        }

        try
        {
            var activityId = _activity.Id;

            // Delete server asset and local file
            if (_activity.Thumbnail != null && _activity.Thumbnail.Id != Guid.Empty)
            {
                await RootObject.Instance.LEE.MediaManager.DeleteMediaFileAsync(activityId, _activity.Thumbnail.Id);
            }

            // Clear thumbnail in activity model and update server activity JSON
            await activityManager.SetThumbnailAsync(null);

            // Reset UI
            _defaultThumbnail.SetActive(true);
            _imgThumbnail.gameObject.SetActive(false);
            SetButtonDeleteThumbnailActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete thumbnail: {ex}");
        }
    } 
    public void OnDeleteStepClick(ActivityStep step, Action deleteCallback = null)
    {
        if (activityManager.Activity?.Steps?.Count > 1)
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
        var worldPos = (baseCamera.transform.forward * 0.5f) + baseCamera.transform.position;
        var anchor = RootObject.Instance.CalibrationManager?.Anchor;
        var position = anchor != null ? anchor.InverseTransformPoint(worldPos) : worldPos;
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
        if (_btnAssignRoomModel != null)
        {
            _btnAssignRoomModel.interactable = value;
            _btnAssignRoomModel.gameObject.SetActive(value);
        }
        if (_btnRepositionDigitalTwin != null)
        {
            _btnRepositionDigitalTwin.interactable = value;
        }
        if (_rowRepositionDigitalTwin != null)
        {
            _rowRepositionDigitalTwin.SetActive(value);
        }
        if (_panelDigitalTwinSettingsRect != null)
        {
            _panelDigitalTwinSettingsRect.sizeDelta = new Vector2(_panelDigitalTwinSettingsRect.sizeDelta.x, value ? 315f : 175f);
        }

        _stepsList.ForEach(t => t.OnEditModeChanged(value));
    }

    private void OnThumbnailButtonPressed()
    {
        var hasThumbnail = _activity != null && _activity.Thumbnail != null && _activity.Thumbnail.Id != Guid.Empty;
        if (hasThumbnail)
        {
            RootView_v2.Instance.dialog.ShowMiddleMultiline(
                "Activity Thumbnail",
                ("Camera", OpenCamera, false),
                ("Gallery", OpenGallery, false),
                ("Delete Thumbnail", () => DeleteThumbnailAsync().Forget(), false),
                ("Cancel", null, true));
        }
        else
        {
            RootView_v2.Instance.dialog.ShowMiddleMultiline(
                "Add Image",
                ("Camera", OpenCamera, false),
                ("Gallery", OpenGallery, false),
                ("Cancel", null, true));
        }
    }
    
    private void OpenCamera()
    {
        Action<Texture2D> onAccept = (texture) => SaveThumbnailAsync(texture).Forget();
        PopupsViewer.Instance.Show(_thumbnailEditorPrefab, onAccept);
    }

    private void OpenGallery()
    {
        PickImage(MAX_PICTURE_SIZE);
    }
    
    /// <summary>
    /// Initiates the process of selecting an image from the device gallery with a maximum resolution constraint.
    /// Saves the selected image temporarily and updates the local thumbnail preview.
    /// </summary>
    /// <param name="maxSize">
    /// The maximum size (width or height) of the selected image in pixels. 
    /// Images larger than this size will be downscaled.
    /// </param>
    /// <remarks>
    /// The image is saved temporarily in the cache folder and used for updating the UI preview.
    /// No server upload is currently performed for the selected image.
    /// For backend upload and full thumbnail management, refer to:
    /// https://github.com/WEKIT-ECS/MIRAGE-XR/issues/2381
    /// </remarks>

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

                float targetAspect = 1f;
                if (_imgThumbnail != null && _imgThumbnail.rectTransform != null)
                {
                    var rect = _imgThumbnail.rectTransform.rect;
                    if (rect.width > 0 && rect.height > 0)
                    {
                        targetAspect = rect.width / rect.height;
                    }
                }

                Texture2D croppedTexture = CropTextureToFitWidth(texture2D, targetAspect, true);
                Destroy(texture2D);

                SaveThumbnailAsync(croppedTexture).Forget();
            }
        });
    }

    private static Texture2D CropTextureToFitWidth(Texture2D source, float targetAspect, bool clipFromTop = true)
    {
        int srcW = source.width;
        int srcH = source.height;

        int cropW = srcW;
        int cropH = Mathf.RoundToInt(srcW / targetAspect);

        int startX = 0;
        int startY = 0;

        if (srcH >= cropH)
        {
            // Taller than target aspect: fit width, clip height from the top (keep bottom rows)
            startY = clipFromTop ? 0 : (srcH - cropH);
        }
        else
        {
            // Wider than target aspect: fit height, center-crop width
            cropH = srcH;
            cropW = Mathf.RoundToInt(srcH * targetAspect);
            cropW = Mathf.Clamp(cropW, 1, srcW);
            startX = (srcW - cropW) / 2;
            startY = 0;
        }

        Color[] pixels = source.GetPixels(startX, startY, cropW, cropH);
        Texture2D cropped = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
        cropped.SetPixels(pixels);
        cropped.Apply();
        return cropped;
    }

    private void OnThumbnailAccepted(string path)
    {
        LoadThumbnail();
    }
    // to
    private void OnActivityNameEndEdit(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            UnityEngine.Debug.Log("[StepsListView_v2]: Description is empty");
            return;
        }
        
        if (activityManager?.Activity != null) 
        {
            activityManager.SetActivityName(title);
            if (_textActivityName)
            {
                _textActivityName.text = title;
            }
            if (_activity != null)
            {
                _activity.Name = title;
            }
        }
        else
        {
            UnityEngine.Debug.LogError("[StepsListView_v2]:OnActivityNameEndEdit failed: activityManager or Activity is null");
        }
    }

    private void OnActivityDescriptionEndEdit(string description)
    {

        if (string.IsNullOrEmpty(description))
        {
            UnityEngine.Debug.Log("[StepsListView_v2]: Description is empty");
            return;
        }
        if (activityManager?.Activity != null)
        {
            activityManager.SetActivityDescription(description);
            if (_activity != null)
            {
                _activity.Description = description;
            }
        }
        else
        {
            UnityEngine.Debug.LogError("[StepsListView_v2]:OnActivityDescriptionEndEdit failed: activityManager or Activity is null");
        }
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
        var isEditMode = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManager.IsEditorMode;
        PopupsViewer.Instance.Show(_calibrationViewPrefab, (Action)OnCalibrationViewOpened, (Action)OnCalibrationViewClosed, isEditMode, false, true, true);
    }

    private void InitDigitalTwinCalibrationButtons()
    {
        if (_btnFloorLevel == null || _btnFloorLevel.transform.parent == null)
        {
            return;
        }

        var parent = _btnFloorLevel.transform.parent;
        var templateCard = _btnWithMarker != null ? _btnWithMarker.gameObject : _btnFloorLevel.gameObject;
        _panelDigitalTwinSettings = Instantiate(templateCard, parent, false);
        _panelDigitalTwinSettings.name = "Panel_DigitalTwinSettings";

        var boxRect = _panelDigitalTwinSettings.GetComponent<RectTransform>();
        _panelDigitalTwinSettingsRect = boxRect;
        if (boxRect != null)
        {
            boxRect.sizeDelta = new Vector2(boxRect.sizeDelta.x, 315f);
        }

        var rootBtn = _panelDigitalTwinSettings.GetComponent<Button>();
        if (rootBtn != null)
        {
            rootBtn.onClick = new Button.ButtonClickedEvent();
            Destroy(rootBtn);
        }

        Transform frameTransform = null;
        Transform arrowTransform = null;
        Transform linkTransform = null;
        TMP_Text headerTemplate = null;

        for (int i = 0; i < _panelDigitalTwinSettings.transform.childCount; i++)
        {
            var child = _panelDigitalTwinSettings.transform.GetChild(i);
            if (child.name == "Image_Frame")
            {
                frameTransform = child;
                var frameImg = child.GetComponent<Image>();
                if (frameImg != null)
                {
                    frameImg.raycastTarget = false;
                }
            }
            else if (child.name == "Image_Arrow")
            {
                arrowTransform = child;
                var arrowImg = child.GetComponent<Image>();
                if (arrowImg != null)
                {
                    arrowImg.raycastTarget = false;
                }
            }
            else if (child.name == "Button_Link")
            {
                linkTransform = child;
            }
            else if (child.name == "Text_Header")
            {
                headerTemplate = child.GetComponent<TMP_Text>();
            }
            else if (child.name == "Text_Description" || child.name == "Image_Marker")
            {
                child.gameObject.SetActive(false);
            }
        }

        // 1. Top Row: "Show Digital Twin" Toggle (like Profile > Caption)
        SwitchToggle templateSwitch = null;
        if (RootView_v2.Instance != null && RootView_v2.Instance.profilePrefab != null)
        {
            var switches = RootView_v2.Instance.profilePrefab.GetComponentsInChildren<SwitchToggle>(true);
            foreach (var sw in switches)
            {
                if (sw != null && sw.transform.parent != null &&
                    sw.transform.parent.name.IndexOf("Caption", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    templateSwitch = sw;
                    break;
                }
            }
            if (templateSwitch == null && switches.Length > 0)
            {
                templateSwitch = switches[0];
            }
        }
        if (templateSwitch == null && _calibrationViewPrefab != null)
        {
            templateSwitch = _calibrationViewPrefab.GetComponentInChildren<SwitchToggle>(true);
        }

        if (templateSwitch != null && templateSwitch.transform.parent != null)
        {
            var toggleRowObj = Instantiate(templateSwitch.transform.parent.gameObject, _panelDigitalTwinSettings.transform, false);
            toggleRowObj.name = "Row_ShowDigitalTwin";
            toggleRowObj.SetActive(true);

            var toggleRowRect = toggleRowObj.GetComponent<RectTransform>();
            if (toggleRowRect != null)
            {
                toggleRowRect.anchorMin = new Vector2(0f, 1f);
                toggleRowRect.anchorMax = new Vector2(1f, 1f);
                toggleRowRect.pivot = new Vector2(0.5f, 0.5f);
                toggleRowRect.anchoredPosition = new Vector2(0f, -45f);
                toggleRowRect.sizeDelta = new Vector2(0f, 55f);
            }

            for (int i = 0; i < toggleRowObj.transform.childCount; i++)
            {
                var c = toggleRowObj.transform.GetChild(i);
                if (c.name == "Background" || c.name == "Divider")
                {
                    c.gameObject.SetActive(false);
                }
            }

            _txtDigitalTwinToggleHeader = toggleRowObj.GetComponentInChildren<TMP_Text>(true);
            if (_txtDigitalTwinToggleHeader != null)
            {
                _txtDigitalTwinToggleHeader.gameObject.SetActive(true);
                _txtDigitalTwinToggleHeader.text = "Show Digital Twin";
                _txtDigitalTwinToggleHeader.fontSize = 34f;
                _txtDigitalTwinToggleHeader.alignment = TextAlignmentOptions.MidlineLeft;
                var labelRect = _txtDigitalTwinToggleHeader.GetComponent<RectTransform>();
                if (labelRect != null)
                {
                    labelRect.anchorMin = Vector2.zero;
                    labelRect.anchorMax = Vector2.one;
                    labelRect.pivot = new Vector2(0.5f, 0.5f);
                    labelRect.offsetMin = new Vector2(30f, 0f);
                    labelRect.offsetMax = new Vector2(-130f, 0f);
                }
            }

            _toggleDigitalTwin = toggleRowObj.GetComponentInChildren<SwitchToggle>(true);
            if (_toggleDigitalTwin != null)
            {
                _toggleDigitalTwin.gameObject.SetActive(true);
                var switchRect = _toggleDigitalTwin.GetComponent<RectTransform>();
                if (switchRect != null)
                {
                    switchRect.anchorMin = new Vector2(1f, 0.5f);
                    switchRect.anchorMax = new Vector2(1f, 0.5f);
                    switchRect.pivot = new Vector2(1f, 0.5f);
                    switchRect.anchoredPosition = new Vector2(-30f, 0f);
                    switchRect.sizeDelta = new Vector2(80f, 50f);
                }

                _toggleDigitalTwinInner = _toggleDigitalTwin.GetComponent<Toggle>() ?? _toggleDigitalTwin.GetComponentInChildren<Toggle>(true);
                if (_toggleDigitalTwinInner != null)
                {
                    int persistentCount = _toggleDigitalTwinInner.onValueChanged.GetPersistentEventCount();
                    for (int i = 0; i < persistentCount; i++)
                    {
                        _toggleDigitalTwinInner.onValueChanged.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                    }
                    _toggleDigitalTwinInner.onValueChanged.RemoveAllListeners();
                }

                _toggleDigitalTwin.EnsureInitialized();
                _toggleDigitalTwin.onValueChanged.AddListener(OnToggleDigitalTwinValueChanged);
            }
        }

        // 2. Row 2: "Digital Twin Style" Selector
        var styleRowObj = new GameObject("Button_DigitalTwinStyle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        styleRowObj.transform.SetParent(_panelDigitalTwinSettings.transform, false);
        var styleRowRect = styleRowObj.GetComponent<RectTransform>();
        styleRowRect.anchorMin = new Vector2(0f, 1f);
        styleRowRect.anchorMax = new Vector2(1f, 1f);
        styleRowRect.pivot = new Vector2(0.5f, 0.5f);
        styleRowRect.anchoredPosition = new Vector2(0f, -105f);
        styleRowRect.sizeDelta = new Vector2(0f, 55f);

        var styleRowImg = styleRowObj.GetComponent<Image>();
        styleRowImg.color = Color.clear;
        styleRowImg.raycastTarget = true;

        _btnDigitalTwinStyle = styleRowObj.GetComponent<Button>();
        _btnDigitalTwinStyle.targetGraphic = styleRowImg;
        _btnDigitalTwinStyle.onClick.AddListener(OnSelectDigitalTwinStyleClicked);

        GameObject repoHeaderObj = null;
        if (headerTemplate != null)
        {
            repoHeaderObj = Instantiate(headerTemplate.gameObject);
            headerTemplate.transform.SetParent(styleRowObj.transform, false);
            headerTemplate.gameObject.SetActive(true);
            headerTemplate.raycastTarget = false;
            headerTemplate.fontSize = 34f;
            headerTemplate.alignment = TextAlignmentOptions.MidlineLeft;
            var headerRect = headerTemplate.GetComponent<RectTransform>();
            if (headerRect != null)
            {
                headerRect.anchorMin = Vector2.zero;
                headerRect.anchorMax = Vector2.one;
                headerRect.pivot = new Vector2(0.5f, 0.5f);
                headerRect.offsetMin = new Vector2(30f, 0f);
                headerRect.offsetMax = new Vector2(-80f, 0f);
            }
            _txtDigitalTwinStyleHeader = headerTemplate;
        }

        GameObject repoArrowObj = null;
        if (arrowTransform != null)
        {
            repoArrowObj = Instantiate(arrowTransform.gameObject);
            arrowTransform.SetParent(styleRowObj.transform, false);
            arrowTransform.gameObject.SetActive(true);
            var arrowRect = arrowTransform.GetComponent<RectTransform>();
            if (arrowRect != null)
            {
                arrowRect.anchorMin = new Vector2(1f, 0.5f);
                arrowRect.anchorMax = new Vector2(1f, 0.5f);
                arrowRect.pivot = new Vector2(0.5f, 0.5f);
                arrowRect.anchoredPosition = new Vector2(-47f, 0f);
                arrowRect.sizeDelta = new Vector2(35f, 35f);
            }
        }

        // 3. Row 3: "Reposition Digital Twin" Row
        var repoRowObj = new GameObject("Button_RepositionDigitalTwin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        _rowRepositionDigitalTwin = repoRowObj;
        repoRowObj.transform.SetParent(_panelDigitalTwinSettings.transform, false);
        var repoRowRect = repoRowObj.GetComponent<RectTransform>();
        repoRowRect.anchorMin = new Vector2(0f, 1f);
        repoRowRect.anchorMax = new Vector2(1f, 1f);
        repoRowRect.pivot = new Vector2(0.5f, 0.5f);
        repoRowRect.anchoredPosition = new Vector2(0f, -165f);
        repoRowRect.sizeDelta = new Vector2(0f, 55f);

        var repoRowImg = repoRowObj.GetComponent<Image>();
        repoRowImg.color = Color.clear;
        repoRowImg.raycastTarget = true;

        _btnRepositionDigitalTwin = repoRowObj.GetComponent<Button>();
        _btnRepositionDigitalTwin.targetGraphic = repoRowImg;
        _btnRepositionDigitalTwin.onClick.AddListener(OpenDigitalTwinRepositionOverlay);
        _btnRepositionDigitalTwin.interactable = activityManager != null && activityManager.IsEditorMode;

        if (repoHeaderObj != null)
        {
            repoHeaderObj.transform.SetParent(repoRowObj.transform, false);
            repoHeaderObj.name = "Text_RepositionHeader";
            repoHeaderObj.SetActive(true);
            var repoHeaderTxt = repoHeaderObj.GetComponent<TMP_Text>();
            if (repoHeaderTxt != null)
            {
                repoHeaderTxt.text = "Reposition Digital Twin";
                repoHeaderTxt.fontSize = 34f;
                repoHeaderTxt.alignment = TextAlignmentOptions.MidlineLeft;
                repoHeaderTxt.raycastTarget = false;
            }
            var repoHeaderRect = repoHeaderObj.GetComponent<RectTransform>();
            if (repoHeaderRect != null)
            {
                repoHeaderRect.anchorMin = Vector2.zero;
                repoHeaderRect.anchorMax = Vector2.one;
                repoHeaderRect.pivot = new Vector2(0.5f, 0.5f);
                repoHeaderRect.offsetMin = new Vector2(30f, 0f);
                repoHeaderRect.offsetMax = new Vector2(-80f, 0f);
            }
        }

        if (repoArrowObj != null)
        {
            repoArrowObj.transform.SetParent(repoRowObj.transform, false);
            repoArrowObj.name = "Image_Arrow_Reposition";
            repoArrowObj.SetActive(true);
            var repoArrowRect = repoArrowObj.GetComponent<RectTransform>();
            if (repoArrowRect != null)
            {
                repoArrowRect.anchorMin = new Vector2(1f, 0.5f);
                repoArrowRect.anchorMax = new Vector2(1f, 0.5f);
                repoArrowRect.pivot = new Vector2(0.5f, 0.5f);
                repoArrowRect.anchoredPosition = new Vector2(-47f, 0f);
                repoArrowRect.sizeDelta = new Vector2(35f, 35f);
            }
        }

        // 4. Bottom Row: "Add Digital Twin..." Link-Style Button (like "Get calibration image")
        if (linkTransform == null && _btnShareImageMarker != null)
        {
            var clonedLink = Instantiate(_btnShareImageMarker.gameObject, _panelDigitalTwinSettings.transform, false);
            clonedLink.name = "Button_Link";
            linkTransform = clonedLink.transform;
        }

        if (linkTransform != null)
        {
            linkTransform.gameObject.SetActive(true);
            var linkRect = linkTransform.GetComponent<RectTransform>();
            if (linkRect != null)
            {
                linkRect.anchorMin = new Vector2(0f, 0f);
                linkRect.anchorMax = new Vector2(1f, 0f);
                linkRect.pivot = new Vector2(0.5f, 0.5f);
                linkRect.anchoredPosition = new Vector2(-15f, 40f);
                linkRect.sizeDelta = new Vector2(-90f, 44f);
            }

            _btnAssignRoomModel = linkTransform.GetComponent<Button>();
            if (_btnAssignRoomModel != null)
            {
                _btnAssignRoomModel.onClick = new Button.ButtonClickedEvent();
                _btnAssignRoomModel.onClick.AddListener(() => OnAssignRoomModelClickedAsync().Forget());
                _btnAssignRoomModel.interactable = activityManager != null && activityManager.IsEditorMode;
            }

            _txtAssignRoomModelLink = linkTransform.GetComponentInChildren<TMP_Text>(true);
            if (_txtAssignRoomModelLink != null)
            {
                _txtAssignRoomModelLink.text = "Add Digital Twin...";
            }
        }

        if (frameTransform != null)
        {
            frameTransform.SetAsLastSibling();
        }

        bool isEditor = activityManager != null && activityManager.IsEditorMode;
        if (_rowRepositionDigitalTwin != null)
        {
            _rowRepositionDigitalTwin.SetActive(isEditor);
        }
        if (_btnAssignRoomModel != null)
        {
            _btnAssignRoomModel.gameObject.SetActive(isEditor);
        }
        if (_panelDigitalTwinSettingsRect != null)
        {
            _panelDigitalTwinSettingsRect.sizeDelta = new Vector2(_panelDigitalTwinSettingsRect.sizeDelta.x, isEditor ? 315f : 175f);
        }

        UpdateDigitalTwinButtonsUI();
    }

    private void UpdateDigitalTwinButtonsUI()
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        bool isShowing = rtm != null && rtm.IsRoomShowing();
        var style = rtm != null ? rtm.GetRoomTwinStyle() : RoomTwinStyle.FullTwin;

        if (_txtDigitalTwinToggleHeader != null)
        {
            _txtDigitalTwinToggleHeader.text = "Show Digital Twin";
        }
        if (_toggleDigitalTwin != null)
        {
            _toggleDigitalTwin.SetIsOnWithoutNotify(isShowing);
            _toggleDigitalTwin.ForceUpdateView();
        }
        else if (_toggleDigitalTwinInner != null)
        {
            _toggleDigitalTwinInner.SetIsOnWithoutNotify(isShowing);
        }

        if (_txtDigitalTwinStyleHeader != null)
        {
            _txtDigitalTwinStyleHeader.text = $"Digital Twin Style: {FormatRoomTwinStyle(style)}";
        }

        if (_txtAssignRoomModelLink != null)
        {
            _txtAssignRoomModelLink.text = "Add Digital Twin...";
        }
    }

    private static string FormatRoomTwinStyle(RoomTwinStyle style)
    {
        switch (style)
        {
            case RoomTwinStyle.TwinVignette:
                return "Twin Vignette";
            case RoomTwinStyle.FullTwin:
                return "Full Twin";
            case RoomTwinStyle.Occlusion:
                return "Occlusion";
            default:
                return style.ToString();
        }
    }

    private void OnToggleDigitalTwinValueChanged(bool isOn)
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        if (rtm == null) return;

        rtm.SetRoomTwinVisibility(isOn);
        UpdateDigitalTwinButtonsUI();
    }

    private void OnSelectDigitalTwinStyleClicked()
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        if (rtm == null || RootView_v2.Instance?.dialog == null) return;

        bool isEditor = activityManager != null && activityManager.IsEditorMode;

        void ApplyStyle(RoomTwinStyle s)
        {
            if (isEditor)
            {
                rtm.UpdateAndPersistStyle(s);
            }
            else
            {
                rtm.SetRoomTwinStyle(s);
            }
            UpdateDigitalTwinButtonsUI();
        }

        RootView_v2.Instance.dialog.ShowMiddleMultiline(
            "Digital Twin Style",
            ("Twin Vignette", () => ApplyStyle(RoomTwinStyle.TwinVignette), false),
            ("Full Twin", () => ApplyStyle(RoomTwinStyle.FullTwin), false),
            ("Occlusion", () => ApplyStyle(RoomTwinStyle.Occlusion), false),
            ("Cancel", null, true));
    }

    private async UniTaskVoid OnAssignRoomModelClickedAsync()
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        if (rtm == null) return;

        if (_digitalTwinGalleryOverlay != null)
        {
            Destroy(_digitalTwinGalleryOverlay);
            _digitalTwinGalleryOverlay = null;
        }

        VirtualInstructorViewMobile viTemplate = null;
        if (RootView_v2.Instance != null && RootView_v2.Instance.editors != null)
        {
            foreach (var editor in RootView_v2.Instance.editors)
            {
                if (editor is VirtualInstructorViewMobile vi)
                {
                    viTemplate = vi;
                    break;
                }
            }
        }

        var replaceModelTemplate = viTemplate != null ? viTemplate.GetComponentInChildren<ReplaceModel>(true) : null;
        if (viTemplate != null && replaceModelTemplate != null && RootView_v2.Instance?.canvas != null)
        {
            await OpenDigitalTwinGalleryFromTemplateAsync(viTemplate, replaceModelTemplate);
            return;
        }

        // Fallback if VirtualInstructorViewMobile template is unavailable
        if (RootView_v2.Instance?.dialog == null) return;

        LoadView.Instance?.Show();
        string[] serverScans = Array.Empty<string>();
        try
        {
            serverScans = await rtm.GetListOfRoomScansAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            LoadView.Instance?.Hide();
        }

        var allScans = new List<string>();
        var localScans = RootObject.Instance?.RoomTwinLibraryManager?.LibraryContent;
        if (localScans != null)
        {
            foreach (var scan in localScans)
            {
                if (!string.IsNullOrEmpty(scan) && !allScans.Contains(scan))
                {
                    allScans.Add(scan);
                }
            }
        }
        if (serverScans != null)
        {
            foreach (var scan in serverScans)
            {
                if (!string.IsNullOrEmpty(scan) && !allScans.Contains(scan))
                {
                    allScans.Add(scan);
                }
            }
        }

        var options = new List<(string text, Action onClick, bool isWarning)>
        {
            ($"Default ({rtm.defaultScanName})", () => LoadDefaultRoomScanAsync().Forget(), false)
        };

        foreach (var scanId in allScans)
        {
            var capturedId = scanId;
            options.Add((capturedId, () => LoadSelectedRoomScanAsync(capturedId).Forget(), false));
        }

        options.Add(("Cancel", null, true));
        RootView_v2.Instance.dialog.ShowMiddleMultiline("Add Digital Twin...", options.ToArray());
    }

    private async UniTask OpenDigitalTwinGalleryFromTemplateAsync(VirtualInstructorViewMobile viTemplate, ReplaceModel replaceModelTemplate)
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        if (rtm == null || RootView_v2.Instance?.canvas == null) return;

        _digitalTwinGalleryOverlay = new GameObject("DigitalTwinGalleryOverlay", typeof(RectTransform));
        _digitalTwinGalleryOverlay.transform.SetParent(RootView_v2.Instance.canvas.transform, false);
        _digitalTwinGalleryOverlay.transform.SetAsLastSibling();

        var overlayRect = _digitalTwinGalleryOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.localScale = Vector3.one;

        // Keep inactive while cloning so ReplaceModel.OnEnable / Start do not execute
        _digitalTwinGalleryOverlay.SetActive(false);

        var panelTemplate = viTemplate.transform.Find("Panel");
        if (panelTemplate != null)
        {
            var panelClone = Instantiate(panelTemplate.gameObject, _digitalTwinGalleryOverlay.transform, false);
            panelClone.name = "Panel";
            panelClone.SetActive(true);

            var settingsPanel = panelClone.transform.Find("SettingsPanel");
            if (settingsPanel != null)
            {
                settingsPanel.gameObject.SetActive(false);
            }

            var header = panelClone.transform.Find("Header");
            if (header != null)
            {
                var btnArrow = header.Find("ButtonArrow");
                if (btnArrow != null)
                {
                    btnArrow.gameObject.SetActive(false);
                }

                var buttons = header.Find("Buttons");
                if (buttons != null)
                {
                    var headerTexts = buttons.GetComponentsInChildren<TMP_Text>(true);
                    if (headerTexts.Length > 0)
                    {
                        headerTexts[0].text = "Add Digital Twin";
                    }

                    var headerBtns = buttons.GetComponentsInChildren<Button>(true);
                    for (int i = 0; i < headerBtns.Length; i++)
                    {
                        if (i == 0)
                        {
                            int persistentCount = headerBtns[i].onClick.GetPersistentEventCount();
                            for (int p = 0; p < persistentCount; p++)
                            {
                                headerBtns[i].onClick.SetPersistentListenerState(p, UnityEngine.Events.UnityEventCallState.Off);
                            }
                            headerBtns[i].onClick = new Button.ButtonClickedEvent();
                            headerBtns[i].onClick.AddListener(CloseDigitalTwinGalleryOverlay);
                        }
                        else
                        {
                            headerBtns[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        var galleryClone = Instantiate(replaceModelTemplate.gameObject, _digitalTwinGalleryOverlay.transform, false);
        galleryClone.name = "DigitalTwinSelectionGallery";
        galleryClone.SetActive(true);

        var clonedReplaceModel = galleryClone.GetComponent<ReplaceModel>();
        if (clonedReplaceModel != null)
        {
            DestroyImmediate(clonedReplaceModel);
        }

        foreach (var selElem in galleryClone.GetComponentsInChildren<GallerySelectionElement>(true))
        {
            if (selElem != null)
            {
                selElem.gameObject.SetActive(false);
            }
        }

        foreach (var addPanel in galleryClone.GetComponentsInChildren<AddModelPanel>(true))
        {
            if (addPanel != null)
            {
                addPanel.gameObject.SetActive(false);
            }
        }

        var toggleGroup = galleryClone.transform.Find("ToggleGroup_Spatial");
        if (toggleGroup != null)
        {
            foreach (var txt in toggleGroup.GetComponentsInChildren<TMP_Text>(true))
            {
                if (txt.text != null && txt.text.IndexOf("character", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    txt.text = "My Digital Twins";
                }
            }
        }

        RoomScanGallery localRoomGallery = null;
        RoomScanGallery serverRoomGallery = null;

        var avatarGalleries = galleryClone.GetComponentsInChildren<AvatarGallery>(true);
        foreach (var avGallery in avatarGalleries)
        {
            if (avGallery == null) continue;
            bool isLocal = avGallery.ElementsDeleteable || avGallery.gameObject.name.IndexOf("My", StringComparison.OrdinalIgnoreCase) >= 0;

            if (avGallery.PlaceholderForEmptyGallery != null)
            {
                foreach (var pTxt in avGallery.PlaceholderForEmptyGallery.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (!string.IsNullOrEmpty(pTxt.text))
                    {
                        pTxt.text = pTxt.text.Replace("characters", "Digital Twins").Replace("character", "Digital Twin").Replace("avatars", "Digital Twins").Replace("avatar", "Digital Twin");
                    }
                }
            }

            var roomGallery = avGallery.gameObject.AddComponent<RoomScanGallery>();
            roomGallery.CopyConfigFrom(avGallery, isLocal);
            DestroyImmediate(avGallery);

            if (isLocal && localRoomGallery == null)
            {
                localRoomGallery = roomGallery;
            }
            else
            {
                serverRoomGallery = roomGallery;
            }
        }

        if (localRoomGallery != null)
        {
            localRoomGallery.ElementSelected += OnLocalDigitalTwinSelected;
        }
        if (serverRoomGallery != null)
        {
            serverRoomGallery.ElementSelected += OnServerDigitalTwinSelected;
        }

        RootView_v2.Instance.HideBaseView();
        _digitalTwinGalleryOverlay.SetActive(true);

        if (localRoomGallery != null)
        {
            var localList = RootObject.Instance?.RoomTwinLibraryManager?.LibraryContent;
            localRoomGallery.ElementIds = localList != null ? new List<string>(localList) : new List<string>();
        }

        if (serverRoomGallery != null)
        {
            try
            {
                var serverScans = await rtm.GetListOfRoomScansAsync();
                if (_digitalTwinGalleryOverlay == null || serverRoomGallery == null) return;
                serverRoomGallery.ElementIds = serverScans != null ? new List<string>(serverScans) : new List<string>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not fetch server room scans: {e.Message}");
                if (serverRoomGallery != null)
                {
                    serverRoomGallery.ElementIds = new List<string>();
                }
            }
        }
    }

    private void OnLocalDigitalTwinSelected(string scanId)
    {
        CloseDigitalTwinGalleryOverlay();
        LoadSelectedRoomScanAsync(scanId).Forget();
    }

    private void OnServerDigitalTwinSelected(string scanId)
    {
        RootObject.Instance?.RoomTwinLibraryManager?.Add(scanId);
        CloseDigitalTwinGalleryOverlay();
        LoadSelectedRoomScanAsync(scanId).Forget();
    }

    private void CloseDigitalTwinGalleryOverlay()
    {
        if (_digitalTwinGalleryOverlay != null)
        {
            Destroy(_digitalTwinGalleryOverlay);
            _digitalTwinGalleryOverlay = null;
        }
        RootView_v2.Instance?.ShowBaseView();
        UpdateDigitalTwinButtonsUI();
    }

    private async UniTaskVoid LoadDefaultRoomScanAsync()
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        if (rtm == null) return;

        LoadView.Instance?.Show();
        try
        {
            await rtm.AssignDigitalTwinAsync(rtm.defaultScanName);
            UpdateDigitalTwinButtonsUI();
        }
        finally
        {
            LoadView.Instance?.Hide();
        }
    }

    private async UniTaskVoid LoadSelectedRoomScanAsync(string scanId)
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        if (rtm == null) return;

        LoadView.Instance?.Show();
        try
        {
            RootObject.Instance?.RoomTwinLibraryManager?.Add(scanId);
            await rtm.AssignDigitalTwinAsync(scanId);
            UpdateDigitalTwinButtonsUI();
        }
        finally
        {
            LoadView.Instance?.Hide();
        }
    }

    private void OpenDigitalTwinRepositionOverlay()
    {
        var rtm = RootObject.Instance?.RoomTwinManager;
        if (rtm == null || RootView_v2.Instance?.canvas == null) return;

        if (_digitalTwinRepositionOverlay != null)
        {
            Destroy(_digitalTwinRepositionOverlay);
            _digitalTwinRepositionOverlay = null;
        }

        var prefab = _digitalTwinRepositionPrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<MirageXR.DigitalTwinRepositionDialog>("Prefabs/UI/Mobile/DigitalTwinRepositionDialog");
        }

        if (prefab != null)
        {
            var dialogInstance = Instantiate(prefab, RootView_v2.Instance.canvas.transform, false);
            dialogInstance.transform.SetAsLastSibling();
            dialogInstance.gameObject.SetActive(true);
            dialogInstance.Init(rtm, CloseDigitalTwinRepositionOverlay);
            _digitalTwinRepositionOverlay = dialogInstance.gameObject;
            return;
        }

        BuildDynamicDigitalTwinRepositionOverlay(rtm);
    }

    private void BuildDynamicDigitalTwinRepositionOverlay(RoomTwinManager rtm)
    {
        var loc = rtm.GetRoomTwinLocation();
        float posXcm = loc.Position.x * 100f;
        float posYcm = loc.Position.y * 100f;
        float posZcm = loc.Position.z * 100f;
        Vector3 rotEuler = loc.Rotation;
        Vector3 scaleVec = loc.Scale == Vector3.zero ? Vector3.one : loc.Scale;

        _digitalTwinRepositionOverlay = new GameObject("DigitalTwinRepositionOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _digitalTwinRepositionOverlay.transform.SetParent(RootView_v2.Instance.canvas.transform, false);
        _digitalTwinRepositionOverlay.transform.SetAsLastSibling();

        var overlayRect = _digitalTwinRepositionOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.localScale = Vector3.one;

        var overlayImg = _digitalTwinRepositionOverlay.GetComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.6f);
        overlayImg.raycastTarget = true;

        // Modal card panel
        var cardObj = new GameObject("CardPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
        cardObj.transform.SetParent(_digitalTwinRepositionOverlay.transform, false);

        var cardImg = cardObj.GetComponent<Image>();
        var settingsImg = _panelDigitalTwinSettings != null ? _panelDigitalTwinSettings.GetComponent<Image>() : null;
        if (settingsImg != null && settingsImg.sprite != null)
        {
            cardImg.sprite = settingsImg.sprite;
            cardImg.type = settingsImg.type;
        }
        cardImg.color = new Color(0.165f, 0.165f, 0.165f, 1f); // #2A2A2A grey scheme
        cardImg.raycastTarget = true;

        var cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(620f, 750f);
        cardRect.anchoredPosition = Vector2.zero;

        var vLayout = cardObj.GetComponent<VerticalLayoutGroup>();
        vLayout.padding = new RectOffset(30, 30, 25, 25);
        vLayout.spacing = 14f;
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = false;
        vLayout.childForceExpandWidth = true;
        vLayout.childForceExpandHeight = false;

        TMP_Text textTemplate = _textActivityName;

        // Title
        var titleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(cardObj.transform, false);
        var titleTxt = titleObj.GetComponent<TMP_Text>();
        if (textTemplate != null)
        {
            titleTxt.font = textTemplate.font;
        }
        titleTxt.text = "Reposition Digital Twin";
        titleTxt.fontSize = 34f;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = Color.white;
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 45f);

        TMP_Text CreateSectionHeader(string title)
        {
            var headerObj = new GameObject($"Header_{title}", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            headerObj.transform.SetParent(cardObj.transform, false);
            var hTxt = headerObj.GetComponent<TMP_Text>();
            if (textTemplate != null) hTxt.font = textTemplate.font;
            hTxt.text = title;
            hTxt.fontSize = 24f;
            hTxt.fontStyle = FontStyles.Bold;
            hTxt.alignment = TextAlignmentOptions.MidlineLeft;
            hTxt.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            headerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 32f);
            return hTxt;
        }

        // Section 1: Position
        CreateSectionHeader("Position Offset (cm)");
        var posInputs = CreateMobileVector3Row(cardObj.transform, new Vector3(posXcm, posYcm, posZcm), textTemplate);

        // Section 2: Rotation
        CreateSectionHeader("Rotation (degrees)");
        var rotInputs = CreateMobileVector3Row(cardObj.transform, rotEuler, textTemplate);

        // Section 3: Scale
        CreateSectionHeader("Scale");
        var scaleInputs = CreateMobileVector3Row(cardObj.transform, scaleVec, textTemplate);

        // Buttons Row
        var btnRowObj = new GameObject("ButtonsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnRowObj.transform.SetParent(cardObj.transform, false);
        var btnRowRect = btnRowObj.GetComponent<RectTransform>();
        btnRowRect.sizeDelta = new Vector2(0f, 65f);

        var btnLayout = btnRowObj.GetComponent<HorizontalLayoutGroup>();
        btnLayout.spacing = 15f;
        btnLayout.childControlWidth = true;
        btnLayout.childControlHeight = true;
        btnLayout.childForceExpandWidth = true;
        btnLayout.childForceExpandHeight = true;

        // Reset
        CreateMobileButton(btnRowObj.transform, "Reset", () =>
        {
            if (posInputs.x != null) posInputs.x.text = "0";
            if (posInputs.y != null) posInputs.y.text = "0";
            if (posInputs.z != null) posInputs.z.text = "0";

            if (rotInputs.x != null) rotInputs.x.text = "0";
            if (rotInputs.y != null) rotInputs.y.text = "0";
            if (rotInputs.z != null) rotInputs.z.text = "0";

            if (scaleInputs.x != null) scaleInputs.x.text = "1";
            if (scaleInputs.y != null) scaleInputs.y.text = "1";
            if (scaleInputs.z != null) scaleInputs.z.text = "1";
        }, new Color(0.23f, 0.23f, 0.23f, 1f));

        // Cancel
        CreateMobileButton(btnRowObj.transform, "Cancel", CloseDigitalTwinRepositionOverlay, new Color(0.23f, 0.23f, 0.23f, 1f));

        // Apply
        CreateMobileButton(btnRowObj.transform, "Apply", () =>
        {
            float px = ParseInvariantFloat(posInputs.x?.text, 0f) * 0.01f;
            float py = ParseInvariantFloat(posInputs.y?.text, 0f) * 0.01f;
            float pz = ParseInvariantFloat(posInputs.z?.text, 0f) * 0.01f;

            float rx = ParseInvariantFloat(rotInputs.x?.text, 0f);
            float ry = ParseInvariantFloat(rotInputs.y?.text, 0f);
            float rz = ParseInvariantFloat(rotInputs.z?.text, 0f);

            float sx = ParseInvariantFloat(scaleInputs.x?.text, 1f);
            float sy = ParseInvariantFloat(scaleInputs.y?.text, 1f);
            float sz = ParseInvariantFloat(scaleInputs.z?.text, 1f);

            Vector3 newPos = new Vector3(px, py, pz);
            Vector3 newScale = new Vector3(sx, sy, sz);
            if (newScale.sqrMagnitude < 0.0001f) newScale = Vector3.one;

            var newLocation = new Location
            {
                Position = newPos,
                Rotation = new Vector3(rx, ry, rz),
                Scale = newScale
            };
            rtm.UpdateAndPersistLocation(newLocation);
            CloseDigitalTwinRepositionOverlay();
        }, new Color(0.494f, 0.290f, 0.753f, 1f)); // #7E4AC0 purple primary
    }

    private void CloseDigitalTwinRepositionOverlay()
    {
        if (_digitalTwinRepositionOverlay != null)
        {
            Destroy(_digitalTwinRepositionOverlay);
            _digitalTwinRepositionOverlay = null;
        }
        UpdateDigitalTwinButtonsUI();
    }

    private (TMP_InputField x, TMP_InputField y, TMP_InputField z) CreateMobileVector3Row(
        Transform parent,
        Vector3 initialValues,
        TMP_Text labelTemplate)
    {
        var rowObj = new GameObject("Vector3Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rowObj.transform.SetParent(parent, false);
        var rowRect = rowObj.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0f, 60f);

        var layout = rowObj.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        TMP_InputField CreateCol(string axis, float val)
        {
            var colObj = new GameObject($"Col_{axis}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            colObj.transform.SetParent(rowObj.transform, false);
            var colLayout = colObj.GetComponent<HorizontalLayoutGroup>();
            colLayout.spacing = 6f;
            colLayout.childControlWidth = true;
            colLayout.childControlHeight = true;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = true;

            var lblObj = new GameObject($"Label_{axis}", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(colObj.transform, false);
            var axisLabel = lblObj.GetComponent<TMP_Text>();
            if (labelTemplate != null) axisLabel.font = labelTemplate.font;

            if (axisLabel != null)
            {
                axisLabel.text = axis;
                axisLabel.fontSize = 26f;
                axisLabel.alignment = TextAlignmentOptions.Center;
                axisLabel.color = Color.white;
                var layoutElem = axisLabel.gameObject.AddComponent<LayoutElement>();
                layoutElem.preferredWidth = 25f;
                layoutElem.minWidth = 20f;
                layoutElem.flexibleWidth = 0f;
            }

            var input = CreateMobileInputField(colObj.transform, val.ToString("0.##", CultureInfo.InvariantCulture), axis);
            if (input != null)
            {
                var layoutElem = input.gameObject.AddComponent<LayoutElement>();
                layoutElem.flexibleWidth = 1f;
                layoutElem.minWidth = 80f;
            }
            return input;
        }

        var inX = CreateCol("X", initialValues.x);
        var inY = CreateCol("Y", initialValues.y);
        var inZ = CreateCol("Z", initialValues.z);

        return (inX, inY, inZ);
    }

    private TMP_InputField CreateMobileInputField(Transform parent, string initialValue, string placeholderText)
    {
        TMP_InputField inputField;
        if (_inputFieldActivityName != null)
        {
            var clone = Instantiate(_inputFieldActivityName.gameObject, parent, false);
            clone.name = $"InputField_{placeholderText}";
            clone.SetActive(true);
            inputField = clone.GetComponent<TMP_InputField>();
        }
        else
        {
            var obj = new GameObject($"InputField_{placeholderText}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
            obj.transform.SetParent(parent, false);
            inputField = obj.GetComponent<TMP_InputField>();
        }

        if (inputField != null)
        {
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onEndEdit.RemoveAllListeners();
            inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.characterLimit = 15;
            inputField.text = initialValue;
            if (inputField.placeholder is TMP_Text placeholderTxt)
            {
                placeholderTxt.text = placeholderText;
            }
            var rect = inputField.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(0f, 55f);
            }
        }
        return inputField;
    }

    private Button CreateMobileButton(Transform parent, string label, Action onClick, Color? bgColor = null)
    {
        var obj = new GameObject($"Button_{label}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        var btn = obj.GetComponent<Button>();
        var img = obj.GetComponent<Image>();
        img.color = bgColor ?? new Color(0.23f, 0.23f, 0.23f, 1f);
        img.raycastTarget = true;

        var txtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(obj.transform, false);
        var txt = txtObj.GetComponent<TMP_Text>();
        if (_textActivityName != null) txt.font = _textActivityName.font;
        txt.text = label;
        txt.fontSize = 26f;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        var txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        btn.onClick.AddListener(() => onClick?.Invoke());
        var rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 60f);
        return btn;
    }

    private static float ParseInvariantFloat(string s, float fallback)
    {
        if (string.IsNullOrWhiteSpace(s)) return fallback;
        s = s.Trim();
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
        {
            return val;
        }
        return fallback;
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
        var stepsCount = activityManager.Activity.Steps.Count;
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

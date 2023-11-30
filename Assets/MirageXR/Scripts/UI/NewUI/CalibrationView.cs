using System;
using System.Threading.Tasks;
using DG.Tweening;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

public class CalibrationView : PopupBase
{
    private static CalibrationManager calibrationManager => RootObject.Instance.calibrationManager;

    private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

    private static PlaneManagerWrapper planeManager => RootObject.Instance.planeManager;

    private static CameraCalibrationChecker cameraCalibrationChecker => RootObject.Instance.cameraCalibrationChecker;

    private static GridManager gridManager => RootObject.Instance.gridManager;

    private string CALIBRATION_TEXT = "Calibration";
    private string SELECT_CALIBRATION_TEXT = "Select calibration type";
    private string NEW_POSITION_TEXT = "New position";
    private string FLOOR_DETECTION_TEXT = "Floor detection";
    private string HINT_MARKER_TEXT = "Look at the calibration image on a printed paper or a screen to calibrate the activity.";
    private string HINT_PLACEMENT_TEXT = "Tap on the plane to place the anchor.";
    private string HINT_FLOOR_TEXT = "Look at the floor while moving your device. As a plane appears, click on it.";
    private int DELAY_TIME = 500;
    private int CLOSE_TIME = 1000;

    [SerializeField] private GameObject _footer;
    [SerializeField] private Image _imageTarget;
    [SerializeField] private Transform _imageCalibrationAnimation;
    [SerializeField] private Transform _imageDetectionAnimation;
    [SerializeField] private TMP_Text _textTop;
    [SerializeField] private TMP_Text _textDone;
    [SerializeField] private TMP_Text _textHint;
    [SerializeField] private GameObject _calibrationAnimation;
    [SerializeField] private GameObject _floorDetectionAnimation;
    [SerializeField] private GameObject _planeAnimation;
    [SerializeField] private GameObject _panelSelectType;
    [SerializeField] private Button _btnImageTarget;
    [SerializeField] private Button _btnManualPlacement;
    [SerializeField] private Toggle _toggleResetPosition;
    [SerializeField] private Button _btnBack;
    [SerializeField] private Button _btnApply;
    [SerializeField] private Color _colorRed;
    [SerializeField] private Color _colorBlue;

    private Action _showBaseView;
    private Action _hideBaseView;
    private bool _isNewPosition;
    private bool _isFloorOnly;
    private Tweener _tweenerCalibration;
    private Sequence _tweenerDetection;
    private Pose _startPose;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        _canBeClosedByOutTap = false;
        _showBackground = false;

        _calibrationAnimation.SetActive(false);
        _floorDetectionAnimation.SetActive(false);
        _panelSelectType.SetActive(false);
        _planeAnimation.SetActive(false);

        _btnBack.onClick.AddListener(OnCloseButtonPressed);
        _btnApply.onClick.AddListener(OnApplyButtonPressed);
        _btnImageTarget.onClick.AddListener(OnButtonImageTargetClicked);
        _btnManualPlacement.onClick.AddListener(OnButtonManualPlacementClicked);
        _toggleResetPosition.isOn = _isNewPosition;
        _toggleResetPosition.onValueChanged.AddListener(OnToggleResetPositionValueChanged);
        _btnApply.gameObject.SetActive(false);

        ResetCalibration();

        _textTop.text = _isNewPosition ? NEW_POSITION_TEXT : CALIBRATION_TEXT;

        calibrationManager.onCalibrationStarted.AddListener(OnCalibrationStarted);
        calibrationManager.onCalibrationCanceled.AddListener(OnCalibrationCanceled);
        calibrationManager.onCalibrationFinished.AddListener(OnCalibrationFinished);

        _hideBaseView?.Invoke();

        cameraCalibrationChecker.StopChecker();
        _startPose = calibrationManager.GetAnchorPositionAsync();

        if (_isFloorOnly || !floorManager.isFloorDetected)
        {
            StartFloorDetectionAsync().AsAsyncVoid();
        }
        else
        {
            ShowSelectTypePanel();
        }
    }

    private void OnButtonImageTargetClicked()
    {
        StartCalibration();
    }

    private void OnButtonManualPlacementClicked()
    {
        StartPlaceCalibrationAsync().AsAsyncVoid();
    }

    private void OnToggleResetPositionValueChanged(bool value)
    {
        _isNewPosition = value;
    }

    private async Task StartFloorDetectionAsync()
    {
        _floorDetectionAnimation.SetActive(true);
        _imageDetectionAnimation.eulerAngles = new Vector3(0, 0, -10);

        _tweenerDetection = DOTween.Sequence();
        _tweenerDetection.Append(_imageDetectionAnimation.DOLocalRotate(new Vector3(0, 0, 10), 1f));
        _tweenerDetection.Append(_imageDetectionAnimation.DOLocalRotate(new Vector3(0, 0, -10), 1f));
        _tweenerDetection.SetLoops(-1);
        _tweenerDetection.SetEase(Ease.Linear);

        _textTop.text = FLOOR_DETECTION_TEXT;
        _textHint.text = HINT_FLOOR_TEXT;

        await Task.Delay(DELAY_TIME);
        planeManager.EnablePlanes();
        planeManager.onPlaneClicked.AddListener(OnFloorDetected);
    }

    private void OnFloorDetected(PlaneId planeId, Vector3 position)
    {
        planeManager.onPlaneClicked.RemoveListener(OnFloorDetected);
        OnFloorDetectedAsync(planeId, position).AsAsyncVoid();
    }

    private async Task OnFloorDetectedAsync(PlaneId planeId, Vector3 position)
    {
        floorManager.SetFloor(planeId, position);
        _floorDetectionAnimation.SetActive(false);
        _tweenerDetection?.Kill();
        await Task.Delay(DELAY_TIME);
        planeManager.DisablePlanes();

        if (_isFloorOnly)
        {
            Close();
        }
        else
        {
            ShowSelectTypePanel();
        }
    }

    private void ShowSelectTypePanel()
    {
        _footer.SetActive(false);
        _textTop.text = SELECT_CALIBRATION_TEXT;
        _panelSelectType.SetActive(true);
    }

    private async Task StartPlaceCalibrationAsync()
    {
        if (_isNewPosition)
        {
            var synchronizer = RootObject.Instance.workplaceManager.detectableContainer.GetComponentInParent<PoseSynchronizer>();
            synchronizer.enabled = false;
        }
        await Task.Delay(DELAY_TIME);
        _textTop.text = _isNewPosition ? NEW_POSITION_TEXT : CALIBRATION_TEXT;
        _footer.gameObject.SetActive(true);
        _panelSelectType.SetActive(false);
        _planeAnimation.SetActive(true);
        planeManager.EnablePlanes();
        _textHint.text = HINT_PLACEMENT_TEXT;
        planeManager.onPlaneClicked.AddListener(OnCalibrationPlaceDetected);
    }

    private void OnCalibrationPlaceDetected(PlaneId planeId, Vector3 position)
    {
        _btnApply.gameObject.SetActive(true);

        var cameraPosition = Camera.main.transform.position;
        var direction = cameraPosition - position;
        direction.Normalize();
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        rotation.x = 0;
        rotation.z = 0;

        calibrationManager.SetAnchorPosition(new Pose(position, rotation));
    }

    private void StartCalibration()
    {
        if (_isNewPosition)
        {
            var synchronizer = RootObject.Instance.workplaceManager.detectableContainer.GetComponentInParent<PoseSynchronizer>();
            synchronizer.enabled = false;
        }
        _textTop.text = _isNewPosition ? NEW_POSITION_TEXT : CALIBRATION_TEXT;
        _footer.gameObject.SetActive(true);
        _panelSelectType.SetActive(false);
        _calibrationAnimation.SetActive(true);
        _textHint.text = HINT_MARKER_TEXT;
        calibrationManager.EnableCalibration(_isNewPosition);
    }

    private void OnCalibrationStarted()
    {
        _footer.SetActive(false);
        _imageTarget.color = _colorBlue;
        _imageCalibrationAnimation.gameObject.SetActive(true);
        _textDone.gameObject.SetActive(false);
        _tweenerCalibration = _imageCalibrationAnimation.transform
            .DOLocalRotate(new Vector3(0, 0, -360), calibrationManager.animationTime, RotateMode.FastBeyond360)
            .SetRelative(true).SetEase(Ease.Linear);
    }

    private void OnCalibrationCanceled()
    {
        ResetCalibration();
    }

    private void OnCalibrationFinished()
    {
        OnCalibrationFinishedAsync().AsAsyncVoid();
    }

    private async Task OnCalibrationFinishedAsync()
    {
        await calibrationManager.ApplyCalibrationAsync(_isNewPosition);
        _textDone.gameObject.SetActive(true);
        _imageTarget.gameObject.SetActive(false);
        _imageCalibrationAnimation.gameObject.SetActive(false);
        _footer.SetActive(false);

        var activityManager = RootObject.Instance.activityManager;
        if (gridManager.gridEnabled && activityManager.EditModeActive)
        {
            gridManager.ShowGrid();
        }

        if (_isNewPosition)
        {
            var synchronizer = RootObject.Instance.workplaceManager.detectableContainer.GetComponentInParent<PoseSynchronizer>();
            synchronizer.enabled = true;
        }

        await Task.Delay(CLOSE_TIME);
        Close();
    }

    private void ResetCalibration()
    {
        _imageTarget.color = _colorRed;
        _imageCalibrationAnimation.gameObject.SetActive(false);
        _textDone.gameObject.SetActive(false);
        _footer.SetActive(true);
        _tweenerCalibration?.Kill();
        _tweenerDetection?.Kill();
    }

    public void OnCloseButtonPressed()
    {
        var pose = calibrationManager.GetAnchorPositionAsync();
        if (pose != _startPose)
        {
            calibrationManager.SetAnchorPosition(_startPose);
            calibrationManager.ApplyCalibrationAsync(false).AsAsyncVoid();
        }

        if (_isNewPosition)
        {
            var synchronizer = RootObject.Instance.workplaceManager.detectableContainer.GetComponentInParent<PoseSynchronizer>();
            synchronizer.enabled = true;
        }

        Close();
    }

    public override void Close()
    {
        planeManager.onPlaneClicked.RemoveListener(OnCalibrationPlaceDetected);
        calibrationManager.DisableCalibration();
        planeManager.DisablePlanes();
        _showBaseView?.Invoke();
        cameraCalibrationChecker.RunChecker();
        base.Close();
    }

    private void OnApplyButtonPressed()
    {
        _btnApply.gameObject.SetActive(false);
        planeManager.onPlaneClicked.RemoveListener(OnCalibrationPlaceDetected);
        planeManager.DisablePlanes();
        OnCalibrationFinishedAsync();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _hideBaseView = (Action)args[0];
            _showBaseView = (Action)args[1];
            _isNewPosition = (bool)args[2];
            _isFloorOnly = (bool)args[3];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

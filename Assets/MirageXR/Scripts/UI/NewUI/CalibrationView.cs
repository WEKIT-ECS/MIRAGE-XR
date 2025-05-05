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
    private static ICalibrationManager calibrationManager => RootObject.Instance.CalibrationManager;

    private static FloorManagerWrapper floorManager => RootObject.Instance.FloorManager;

    private static PlaneManagerWrapper planeManager => RootObject.Instance.PlaneManager;

    private static CameraCalibrationChecker cameraCalibrationChecker => RootObject.Instance.CameraCalibrationChecker;

    private static GridManager gridManager => RootObject.Instance.GridManager;

    private string CALIBRATION_TEXT = "Calibration";
    private string SELECT_CALIBRATION_TEXT = "Select calibration type";
    private string NEW_POSITION_TEXT = "New position";
    private string FLOOR_DETECTION_TEXT = "Floor detection";
    private string HINT_MARKER_TEXT = "Look at the calibration image on a printed paper or a screen to calibrate the activity.";
    private string HINT_PLACEMENT_TEXT = "Tap on the plane to place the anchor.";
    private string HINT_FLOOR_TEXT = "Look at the floor while moving your device. As a plane appears, click on it.";
    private string HINT_MOVE_ORIGIN = "The origin is the single main anchor point of the activity. 'Set origin' only if you are first-time editing the activity.";
    private string HINT_RESTORE_POSITION = "Positions are the locations of all contents relative to the origin. ‘Restore positions’ if you view an activity or re-edit.";
    private int DELAY_TIME = 250;

    [SerializeField] private Image _imageTarget;
    [SerializeField] private Transform _imageCalibrationAnimation;
    [SerializeField] private Transform _imageDetectionAnimation;
    [SerializeField] private TMP_Text _textDone;
    [SerializeField] private TMP_Text _textDescroptionImage;
    [SerializeField] private TMP_Text _textDescroptionManual;
    [SerializeField] private GameObject _panelFloor;
    [SerializeField] private GameObject _panelFloorAnimation;
    [SerializeField] private GameObject _panelImage;
    [SerializeField] private GameObject _panelManual;
    [SerializeField] private Toggle _toggleManualResetPosition;
    [SerializeField] private Toggle _toggleImageResetPosition;
    [SerializeField] private Button _btnFloorBack;
    [SerializeField] private Button _btnManualBack;
    [SerializeField] private Button _btnImageBack;
    [SerializeField] private Button _btnApply;
    [SerializeField] private Button _btnFloorNext;
    [SerializeField] private Color _colorRed;
    [SerializeField] private Color _colorBlue;

    private Action _showBaseView;
    private Action _hideBaseView;
    private bool _isMoveOrigin;
    private bool _isFloorOnly;
    private bool _isMarkerLess;
    private Tweener _tweenerCalibration;
    private Sequence _tweenerDetection;
    private Pose _startPose;
    //private LearningExperienceEngine.PoseSynchronizer _poseSynchronizer;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        //_poseSynchronizer = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.detectableContainer.GetComponentInParent<LearningExperienceEngine.PoseSynchronizer>();
        _canBeClosedByOutTap = false;
        _showBackground = false;

        _panelFloor.SetActive(false);
        _panelImage.SetActive(false);
        _panelManual.SetActive(false);

        _btnFloorNext.gameObject.SetActive(false);
        _btnFloorNext.onClick.AddListener(OnFloorNextButtonClicked);

        _btnFloorBack.onClick.AddListener(OnCloseButtonPressed);
        _btnManualBack.onClick.AddListener(OnCloseButtonPressed);
        _btnImageBack.onClick.AddListener(OnCloseButtonPressed);
        _btnApply.onClick.AddListener(OnApplyButtonPressed);
        _toggleManualResetPosition.isOn = !_isMoveOrigin;
        _toggleImageResetPosition.isOn = !_isMoveOrigin;
        _toggleManualResetPosition.onValueChanged.AddListener(OnToggleResetPositionValueChanged);
        _toggleImageResetPosition.onValueChanged.AddListener(OnToggleResetPositionValueChanged);
        _textDescroptionImage.text = _isMoveOrigin ? HINT_MOVE_ORIGIN : HINT_RESTORE_POSITION;
        _textDescroptionManual.text = _isMoveOrigin ? HINT_MOVE_ORIGIN : HINT_RESTORE_POSITION;
        //_poseSynchronizer.enabled = !_isMoveOrigin;
        
        _btnApply.gameObject.SetActive(false);

        ResetCalibration();

        calibrationManager.OnCalibrationStarted.AddListener(OnCalibrationStarted);
        calibrationManager.OnCalibrationCanceled.AddListener(OnCalibrationCanceled);
        calibrationManager.OnCalibrationFinished.AddListener(OnCalibrationFinished);

        _hideBaseView?.Invoke();

        cameraCalibrationChecker.StopChecker();
        _startPose = calibrationManager.GetAnchorPositionAsync();

        if (_isFloorOnly || !floorManager.isFloorDetected)
        {
            StartFloorDetectionAsync().AsAsyncVoid();
        }
        else
        {
            if (_isMarkerLess)
            {
                StartPlaceCalibrationAsync().AsAsyncVoid();
            }
            else
            {
                StartCalibration();
            }
        }
    }

    private void OnToggleResetPositionValueChanged(bool value)
    {
        _isMoveOrigin = !value;
        //_poseSynchronizer.enabled = !_isMoveOrigin;
        _textDescroptionImage.text = _isMoveOrigin ? HINT_MOVE_ORIGIN : HINT_RESTORE_POSITION;
        _textDescroptionManual.text = _isMoveOrigin ? HINT_MOVE_ORIGIN : HINT_RESTORE_POSITION;
    }

    private async Task StartFloorDetectionAsync()
    {
        _panelFloor.SetActive(true);
        _panelFloorAnimation.SetActive(true);
        _imageDetectionAnimation.eulerAngles = new Vector3(0, 0, -10);

        _tweenerDetection = DOTween.Sequence();
        _tweenerDetection.Append(_imageDetectionAnimation.DOLocalRotate(new Vector3(0, 0, 10), 1f));
        _tweenerDetection.Append(_imageDetectionAnimation.DOLocalRotate(new Vector3(0, 0, -10), 1f));
        _tweenerDetection.SetLoops(-1);
        _tweenerDetection.SetEase(Ease.Linear);

        await Task.Delay(DELAY_TIME);
        planeManager.EnablePlanes();
        planeManager.onPlaneClicked.AddListener(OnFloorDetected);
    }

    private void OnFloorDetected(PlaneId planeId, Vector3 position)
    {
        planeManager.onPlaneClicked.RemoveListener(OnFloorDetected);
        OnFloorDetectedAsync(planeId, position);
    }

    private void OnFloorDetectedAsync(PlaneId planeId, Vector3 position)
    {
        floorManager.SetFloor(planeId, position);
        _panelFloorAnimation.SetActive(false);
        _btnFloorNext.gameObject.SetActive(true);
    }

    private void OnFloorNextButtonClicked()
    {
        OnFloorNextButtonClickedAsync().AsAsyncVoid();
    }

    private async Task OnFloorNextButtonClickedAsync()
    {
        _tweenerDetection?.Kill();
        _panelFloor.SetActive(false);
        await Task.Delay(DELAY_TIME);
        planeManager.DisablePlanes();

        if (_isFloorOnly)
        {
            Close();
        }
        else
        {
            if (_isMarkerLess)
            {
                StartPlaceCalibrationAsync().AsAsyncVoid();
            }
            else
            {
                StartCalibration();
            }
        }
    }

    private async Task StartPlaceCalibrationAsync()
    {
        await Task.Delay(DELAY_TIME);
        _panelManual.SetActive(true);
        planeManager.EnablePlanes();
        planeManager.onPlaneClicked.AddListener(OnCalibrationPlaceDetected);
    }

    private void OnCalibrationPlaceDetected(PlaneId planeId, Vector3 position)
    {
        _btnApply.gameObject.SetActive(true);
        var baseCamera = RootObject.Instance.ViewManager.Camera;
        var cameraPosition = baseCamera.transform.position;
        var direction = cameraPosition - position;
        direction.Normalize();
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        rotation.x = 0;
        rotation.z = 0;

        calibrationManager.SetAnchorPosition(new Pose(position, rotation), _isMoveOrigin);
    }

    private void StartCalibration()
    {
        _panelImage.SetActive(true);
        calibrationManager.EnableCalibration(_isMoveOrigin);
    }

    private void OnCalibrationStarted()
    {
        _imageTarget.color = _colorBlue;
        _imageCalibrationAnimation.gameObject.SetActive(true);
        _textDone.gameObject.SetActive(false);
        _tweenerCalibration = _imageCalibrationAnimation.transform
            .DOLocalRotate(new Vector3(0, 0, -360), calibrationManager.AnimationTime, RotateMode.FastBeyond360)
            .SetRelative(true).SetEase(Ease.Linear);
    }

    private void OnCalibrationCanceled()
    {
        ResetCalibration();
    }

    private void OnCalibrationFinished()
    {
        calibrationManager.ApplyCalibration(_isMoveOrigin);
        _textDone.gameObject.SetActive(true);
        _imageTarget.gameObject.SetActive(false);
        _imageCalibrationAnimation.gameObject.SetActive(false);

        var activityManager = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManager;
        if (gridManager.gridEnabled && activityManager.IsEditorMode)
        {
            gridManager.ShowGrid();
        }

        //_poseSynchronizer.enabled = true;

        LearningExperienceEngine.EventManager.WorkplaceCalibrated();
        Close();

        TutorialManager.Instance.InvokeEvent(TutorialManager.TutorialEvent.CALIBRATION_FINISHED);
    }

    private void ResetCalibration()
    {
        _imageTarget.color = _colorRed;
        _imageCalibrationAnimation.gameObject.SetActive(false);
        _textDone.gameObject.SetActive(false);
        _tweenerCalibration?.Kill();
        _tweenerDetection?.Kill();
    }

    public void OnCloseButtonPressed()
    {
        var pose = calibrationManager.GetAnchorPositionAsync();
        if (pose != _startPose)
        {
            calibrationManager.SetAnchorPosition(_startPose, false);
            calibrationManager.ApplyCalibration(false);
        }

        if (_isMoveOrigin)
        {
            //var synchronizer = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.detectableContainer.GetComponentInParent<LearningExperienceEngine.PoseSynchronizer>();
            //synchronizer.enabled = true;
            
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
        OnCalibrationFinished();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _hideBaseView = (Action)args[0];
            _showBaseView = (Action)args[1];
            _isMoveOrigin = (bool)args[2];
            _isFloorOnly = (bool)args[3];
            _isMarkerLess = (bool)args[4];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

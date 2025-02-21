using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using TMPro;
using UnityEngine;

public class CalibrationFlow : MonoBehaviour
{
    private static ICalibrationManager calibrationManager => RootObject.Instance.CalibrationManager;

    private static FloorManagerWrapper floorManager => RootObject.Instance.FloorManager;

    private static PlaneManagerWrapper planeManager => RootObject.Instance.PlaneManager;

    private static GridManager gridManager => RootObject.Instance.GridManager;

    private static CameraCalibrationChecker cameraCalibrationChecker => RootObject.Instance.CameraCalibrationChecker;


    private string CALIBRATION_TEXT = "Calibration";
    private string SELECT_CALIBRATION_TEXT = "Select calibration type";
    private string NEW_POSITION_TEXT = "New position";
    private string FLOOR_DETECTION_TEXT = "Floor detection";
    private string HINT_MARKER_TEXT = "Look at the calibration image on a printed paper or a screen to calibrate the activity.";
    private string HINT_PLACEMENT_TEXT = "Tap on the plane to place the anchor.";
    private string HINT_FLOOR_TEXT = "Look at the floor while moving your device. As a plane appears, click on it.";
    private string DONE_TEXT = "Done";
    private int DELAY_TIME = 500;
    private int CLOSE_TIME = 1000;

    [SerializeField] private TMP_Text _textLabel;
    [SerializeField] private TMP_Text _textMain;
    [SerializeField] private Interactable _btnClose;
    [SerializeField] private FollowMeToggle _followMeToggle;
    [SerializeField] private Interactable _btnImageTarget;
    [SerializeField] private Interactable _btnManualPlacement;
    [SerializeField] private Interactable _btnApply;

    private Pose _startPose;
    private System.Action _onCloseAction;
    private bool _isNewPosition;

    public void Initialization(System.Action onCloseAction)
    {
        _onCloseAction = onCloseAction;
        _followMeToggle.SetFollowMeBehavior(true);

        _startPose = calibrationManager.GetAnchorPositionAsync();
        calibrationManager.OnCalibrationFinished.AddListener(OnCalibrationFinished);

        _btnApply.gameObject.SetActive(false);
        _btnImageTarget.gameObject.SetActive(false);
        _btnManualPlacement.gameObject.SetActive(false);

        _btnClose.OnClick.AddListener(OnCloseButtonPressed);
        _btnImageTarget.OnClick.AddListener(OnButtonImageTargetClicked);
        _btnManualPlacement.OnClick.AddListener(OnButtonManualPlacementClicked);
        _btnApply.OnClick.AddListener(OnApplyButtonPressed);

        cameraCalibrationChecker.RunChecker();
        StartFloorDetectionAsync().AsAsyncVoid();
    }

    private async Task StartFloorDetectionAsync()
    {
        _textLabel.text = FLOOR_DETECTION_TEXT;
        _textMain.text = HINT_FLOOR_TEXT;

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
        _textMain.text = DONE_TEXT;
        await Task.Delay(DELAY_TIME);
        planeManager.DisablePlanes();
        _btnImageTarget.gameObject.SetActive(true);
        _btnManualPlacement.gameObject.SetActive(true);
    }

    private void StartCalibration()
    {
        _textLabel.text = _isNewPosition ? NEW_POSITION_TEXT : CALIBRATION_TEXT;
        _textMain.text = HINT_MARKER_TEXT;
        _btnImageTarget.gameObject.SetActive(false);
        _btnManualPlacement.gameObject.SetActive(false);
        calibrationManager.EnableCalibration(true);
    }

    private void OnCalibrationFinished()
    {
        OnCalibrationFinishedAsync().AsAsyncVoid();
    }

    private async Task OnCalibrationFinishedAsync()
    {
        await calibrationManager.ApplyCalibrationAsync(false);
        var activityManager = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        if (gridManager.gridEnabled && activityManager.EditModeActive)
        {
            gridManager.ShowGrid();
        }

        LearningExperienceEngine.EventManager.WorkplaceCalibrated();
        _textMain.text = DONE_TEXT;
        await Task.Delay(CLOSE_TIME);
        Close();
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

    private void OnButtonImageTargetClicked()
    {
        StartCalibration();
    }

    private void OnButtonManualPlacementClicked()
    {
        StartPlaceCalibrationAsync().AsAsyncVoid();
    }

    private async Task StartPlaceCalibrationAsync()
    {
        await Task.Delay(DELAY_TIME);
        _textLabel.text = _isNewPosition ? NEW_POSITION_TEXT : CALIBRATION_TEXT;
        _btnImageTarget.gameObject.SetActive(false);
        _btnManualPlacement.gameObject.SetActive(false);
        planeManager.EnablePlanes();
        _textMain.text = HINT_PLACEMENT_TEXT;
        planeManager.onPlaneClicked.AddListener(OnCalibrationPlaceDetected);
    }

    private void OnApplyButtonPressed()
    {
        _btnApply.gameObject.SetActive(false);
        planeManager.onPlaneClicked.RemoveListener(OnCalibrationPlaceDetected);
        planeManager.DisablePlanes();
        OnCalibrationFinishedAsync().AsAsyncVoid();
    }

    public void OnCloseButtonPressed()
    {
        var pose = calibrationManager.GetAnchorPositionAsync();
        if (pose != _startPose)
        {
            calibrationManager.SetAnchorPosition(_startPose);
            calibrationManager.ApplyCalibrationAsync(false).AsAsyncVoid();
        }

        Close();
    }

    public void Close()
    {
        planeManager.onPlaneClicked.RemoveListener(OnCalibrationPlaceDetected);
        calibrationManager.DisableCalibration();
        planeManager.DisablePlanes();
        _onCloseAction?.Invoke();
        cameraCalibrationChecker.StopChecker();
        Destroy(gameObject);
    }
}

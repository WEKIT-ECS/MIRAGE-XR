using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using TMPro;
using UnityEngine;

public class CalibrationFlow : MonoBehaviour
{
    private static CalibrationManager calibrationManager => RootObject.Instance.calibrationManager;

    private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

    private static GridManager gridManager => RootObject.Instance.gridManager;

    private string FLOOR_DETECTION_TEXT = "Floor Detection";
    private string CALIBRATION_TEXT = "Calibration";
    private string HINT_MARKER_TEXT = "Look at the calibration image on a printed paper or a screen to calibrate the activity.";
    private string HINT_FLOOR_TEXT = "Look at the floor and mark the floor with a pinch gesture.";
    private string DONE_TEXT = "Done";
    private int DELAY_TIME = 500;
    private int CLOSE_TIME = 1000;

    [SerializeField] private TMP_Text _textLabel;
    [SerializeField] private TMP_Text _textMain;
    [SerializeField] private Interactable _btnClose;
    [SerializeField] private FollowMeToggle _followMeToggle;

    private System.Action _onCloseAction;

    public void Initialization(System.Action onCloseAction)
    {
        _onCloseAction = onCloseAction;
        ResetCalibration();
        _followMeToggle.SetFollowMeBehavior(true);
        _btnClose.OnClick.AddListener(Close);
        calibrationManager.onCalibrationStarted.AddListener(OnCalibrationStarted);
        calibrationManager.onCalibrationCanceled.AddListener(OnCalibrationCanceled);
        calibrationManager.onCalibrationFinished.AddListener(OnCalibrationFinished);

        StartFloorDetectionAsync().AsAsyncVoid();
    }

    private async Task StartFloorDetectionAsync()
    {
        _textLabel.text = FLOOR_DETECTION_TEXT;
        _textMain.text = HINT_FLOOR_TEXT;

        await Task.Delay(DELAY_TIME);
        floorManager.EnableFloorDetection(OnFloorDetected);
    }

    private void OnFloorDetected()
    {
        OnFloorDetectedAsync().AsAsyncVoid();
    }

    private async Task OnFloorDetectedAsync()
    {
        _textMain.text = DONE_TEXT;
        await Task.Delay(DELAY_TIME);
        floorManager.DisableFloorDetection();
        StartCalibration();
    }

    private void StartCalibration()
    {
        _textLabel.text = CALIBRATION_TEXT;
        _textMain.text = HINT_MARKER_TEXT;
        calibrationManager.EnableCalibration();
    }

    private void OnCalibrationStarted()
    {
        
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
        var activityManager = RootObject.Instance.activityManager;
        if (gridManager.gridEnabled && activityManager.EditModeActive)
        {
            gridManager.ShowGrid();
        }

        _textMain.text = DONE_TEXT;
        await Task.Delay(CLOSE_TIME);
        Close();
    }

    private void ResetCalibration()
    {
        
    }

    public void Close()
    {
        calibrationManager.DisableCalibration();
        floorManager.DisableFloorDetection();
        _onCloseAction?.Invoke();
        Destroy(gameObject);
    }
}

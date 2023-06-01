using System;
using System.Threading.Tasks;
using DG.Tweening;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using Action = System.Action;

public class CalibrationView : PopupBase
{
    private static CalibrationManager calibrationManager => RootObject.Instance.calibrationManager;

    private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

    private static GridManager gridManager => RootObject.Instance.gridManager;

    private string CALIBRATION_TEXT = "Calibration";
    private string NEW_POSITION_TEXT = "New position";
    private string HINT_MARKER_TEXT = "Look at the calibration image on a printed paper or a screen to calibrate the activity.";
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
    [SerializeField] private Button _btnBack;
    [SerializeField] private Color _colorRed;
    [SerializeField] private Color _colorBlue;

    private Action _showBaseView;
    private Action _hideBaseView;
    private bool _isNewPosition;
    private Tweener _tweenerCalibration;
    private Sequence _tweenerDetection;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        _canBeClosedByOutTap = false;
        _showBackground = false;
        _btnBack.onClick.AddListener(Close);

        ResetCalibration();

        _textTop.text = _isNewPosition ? NEW_POSITION_TEXT : CALIBRATION_TEXT;

        calibrationManager.onCalibrationStarted.AddListener(OnCalibrationStarted);
        calibrationManager.onCalibrationCanceled.AddListener(OnCalibrationCanceled);
        calibrationManager.onCalibrationFinished.AddListener(OnCalibrationFinished);

        _hideBaseView?.Invoke();

        StartFloorDetectionAsync().AsAsyncVoid();
    }

    private async Task StartFloorDetectionAsync()
    {
        _calibrationAnimation.SetActive(false);
        _floorDetectionAnimation.SetActive(true);
        _imageDetectionAnimation.eulerAngles = new Vector3(0, 0, -10);

        _tweenerDetection = DOTween.Sequence();
        _tweenerDetection.Append(_imageDetectionAnimation.DOLocalRotate(new Vector3(0, 0, 10), 1f));
        _tweenerDetection.Append(_imageDetectionAnimation.DOLocalRotate(new Vector3(0, 0, -10), 1f));
        _tweenerDetection.SetLoops(-1);
        _tweenerDetection.SetEase(Ease.Linear);

        _textHint.text = HINT_FLOOR_TEXT;

        await Task.Delay(DELAY_TIME);
        floorManager.EnableFloorDetection(OnFloorDetected);
    }

    private void OnFloorDetected()
    {
        OnFloorDetectedAsync().AsAsyncVoid();
    }

    private async Task OnFloorDetectedAsync()
    {
        _floorDetectionAnimation.SetActive(false);
        _tweenerDetection?.Kill();
        await Task.Delay(DELAY_TIME);
        floorManager.DisableFloorDetection();
        StartCalibration();
    }

    private void StartCalibration()
    {
        _calibrationAnimation.SetActive(true);
        _floorDetectionAnimation.SetActive(false);
        _textHint.text = HINT_FLOOR_TEXT;
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
        _textDone.gameObject.SetActive(true);
        _imageTarget.gameObject.SetActive(false);
        _imageCalibrationAnimation.gameObject.SetActive(false);
        _footer.SetActive(false);

        if (gridManager.gridEnabled)
        {
            gridManager.ShowGrid();
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

    public override void Close()
    {
        calibrationManager.DisableCalibration();
        floorManager.DisableFloorDetection();
        _showBaseView?.Invoke();
        base.Close();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _hideBaseView = (Action)args[0];
            _showBaseView = (Action)args[1];
            _isNewPosition = (bool)args[2];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

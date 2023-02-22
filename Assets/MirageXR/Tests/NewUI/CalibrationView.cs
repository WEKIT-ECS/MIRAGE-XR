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
    private string CALIBRATION_TEXT = "Calibration";
    private string NEW_POSITION_TEXT = "New position";
    private int CLOSE_TIME = 1000;

    [SerializeField] private GameObject _footer;
    [SerializeField] private Image _imageTarget;
    [SerializeField] private Image _imageAnimation;
    [SerializeField] private TMP_Text _textTop;
    [SerializeField] private TMP_Text _textDone;
    [SerializeField] private Button _btnBack;
    [SerializeField] private Color _colorRed;
    [SerializeField] private Color _colorBlue;

    private Action _showBaseView;
    private Action _hideBaseView;
    private bool _isNewPosition;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        canBeClosedByOutTap = false;
        _btnBack.onClick.AddListener(Close);

        Reset();

        _textTop.text = _isNewPosition ? NEW_POSITION_TEXT : CALIBRATION_TEXT;
        var calibrationTool = CalibrationTool.Instance;
        calibrationTool.isNewPosition = _isNewPosition;
        calibrationTool.onTargetFound.AddListener(OnTargetFound);
        calibrationTool.onTargetLost.AddListener(OnTargetLost);
        calibrationTool.onCalibrationFinished.AddListener(OnCalibrationFinished);
        calibrationTool.isEnabled = true;
        _hideBaseView?.Invoke();
    }

    private void OnTargetFound()
    {
        _footer.SetActive(false);
        _imageTarget.color = _colorBlue;
        _imageAnimation.gameObject.SetActive(true);
        _textDone.gameObject.SetActive(false);
        _imageAnimation.transform
            .DOLocalRotate(new Vector3(0, 0, -360), CalibrationTool.Instance.animationTime, RotateMode.FastBeyond360)
            .SetRelative(true).SetEase(Ease.Linear);
    }

    private void OnTargetLost()
    {
        Reset();
    }

    private void OnCalibrationFinished()
    {
        OnCalibrationFinishedAsync().AsAsyncVoid();
    }

    private async Task OnCalibrationFinishedAsync()
    {
        _textDone.gameObject.SetActive(true);
        _imageTarget.gameObject.SetActive(false);
        _imageAnimation.gameObject.SetActive(false);
        _footer.SetActive(false);
        await Task.Delay(CLOSE_TIME);
        Close();
    }

    private void Reset()
    {
        _imageTarget.color = _colorRed;
        _imageAnimation.gameObject.SetActive(false);
        _textDone.gameObject.SetActive(false);
        _footer.SetActive(true);
    }

    public override void Close()
    {
        var calibrationTool = CalibrationTool.Instance;
        calibrationTool.isEnabled = false;
        calibrationTool.isNewPosition = false;
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

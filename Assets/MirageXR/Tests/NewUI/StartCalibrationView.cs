using UnityEngine;
using UnityEngine.UI;

public class StartCalibrationView : PopupBase
{
    [SerializeField] private Button _btnStartCalibration;
    [SerializeField] private Button _btnGetCalibrationImage;
    [SerializeField] private CalibrationView _calibrationPrefab;

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _btnStartCalibration.onClick.AddListener(OpenCalibrationScreen);
        _btnGetCalibrationImage.onClick.AddListener(GetCalibrationImage);
    }

    private void GetCalibrationImage()
    {
        // TODO
    }

    private void OpenCalibrationScreen()
    {
        PopupsViewer.Instance.Show(_calibrationPrefab);

        // hide bottom panel and new activity screen
        RootView_v2.Instance.bottomPanel.gameObject.SetActive(false);
        RootView_v2.Instance.newActivityPanel.SetActive(false);

        Close();
    }
}

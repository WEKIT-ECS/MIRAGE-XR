using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartCalibrationView : PopupBase
{
    [SerializeField] private Button _btnStartCalibration;
    [SerializeField] private Button _btnGetCalibrationImage;

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
        RootView_v2.Instance.OnStartCalibration();

        // hide bottom panel
        RootView_v2.Instance.bottomPanel.gameObject.SetActive(false);
        Close();
    }
}

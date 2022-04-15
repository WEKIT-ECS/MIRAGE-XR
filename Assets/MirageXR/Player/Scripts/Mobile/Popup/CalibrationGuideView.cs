using System;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationGuideView : PopupBase
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Toggle _toggleDontShow;

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        _btnClose.onClick.AddListener(Close);
        _toggleDontShow.onValueChanged.AddListener(OnToggleValueChanged);
        _toggleDontShow.isOn = DBManager.dontShowCalibrationGuide;
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void OnToggleValueChanged(bool value)
    {
        DBManager.dontShowCalibrationGuide = value;
    }
    
}

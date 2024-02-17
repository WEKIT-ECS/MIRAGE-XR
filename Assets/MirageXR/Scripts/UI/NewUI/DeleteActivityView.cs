using System;
using UnityEngine;
using UnityEngine.UI;

public class DeleteActivityView : PopupBase
{
    [SerializeField] private Toggle _toggleFromDevice;
    [SerializeField] private Toggle _toggleFromServer;
    [SerializeField] private Button _btnOk;
    [SerializeField] private Button _btnCancel;

    private Action<bool, bool> _callback;

    public override void Initialization(Action<PopupBase> onClose, params object[] args) // args: _stepCount, _from, _to, _callback
    {
        base.Initialization(onClose, args);

        _btnOk.onClick.AddListener(OnButtonOkClicked);
        _btnCancel.onClick.AddListener(OnButtonCancelClicked);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _callback = (Action<bool, bool>)args[0];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void OnButtonOkClicked()
    {
        _callback.Invoke(_toggleFromDevice.isOn, _toggleFromServer.isOn);
        Close();
    }

    private void OnButtonCancelClicked()
    {
        Close();
    }
}

using System;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class OpenActivityModeSelect : PopupBase
{
    [SerializeField] private Toggle _editToggle;
    [SerializeField] private Toggle _viewToggle;
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;

    private ActivityListItem_v2 _connectedObject;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _editToggle.onValueChanged.AddListener(OnEditToggle);
        _viewToggle.onValueChanged.AddListener(OnViewToggle);

        _openButton.onClick.AddListener(OnOpen);
        _closeButton.onClick.AddListener(Close);
    }

    private void OnEditToggle(bool value)
    {
        _viewToggle.enabled = !value;
    }

    private void OnViewToggle(bool value)
    {
        _editToggle.enabled = !value;
    }

    private void OnOpen()
    {
        if (_connectedObject)
        {
            _connectedObject.OpenActivity(_editToggle.enabled);
        }

        Close();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        if (args is { Length: 1 } && args[0] is ActivityListItem_v2 obj)
        {
            _connectedObject = obj;
            return true;
        }

        return false;
    }

}

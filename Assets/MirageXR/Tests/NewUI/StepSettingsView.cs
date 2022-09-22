using System;
using UnityEngine;
using UnityEngine.UI;

public class StepSettingsView : PopupBase
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnCopyStep;
    [SerializeField] private Button _btnDeleteStep;

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);

        _btnCopyStep.onClick.AddListener(CopyStep);
        _btnDeleteStep.onClick.AddListener(DeleteStep);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void CopyStep()
    {
        throw new NotImplementedException();
    }

    private void DeleteStep()
    {
        throw new NotImplementedException();
    }
}

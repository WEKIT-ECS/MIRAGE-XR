using System;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

public abstract class PopupBase : MonoBehaviour
{
    protected Action<PopupBase> _onClose;
    protected bool _canBeClosedByOutTap = true;
    protected bool _showBackground = true;

    private bool _isMarkedToDelete = false;

    public bool canBeClosedByOutTap => _canBeClosedByOutTap;

    public bool isMarkedToDelete => _isMarkedToDelete;

    public bool showBackground => _showBackground;

    public virtual void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _onClose = onClose;
        if (!TryToGetArguments(args))
        {
            AppLog.LogError("error when trying to get arguments!");
        }
    }

    public virtual void Close()
    {
        _isMarkedToDelete = true;
        _onClose?.Invoke(this);
    }

    protected abstract bool TryToGetArguments(params object[] args);
}

using System;
using UnityEngine;

public abstract class PopupBase : MonoBehaviour
{
    private Action<PopupBase> _onClose;
    
    [HideInInspector] public bool canBeClosedByOutTap = true;

    public bool isMarkedToDelete { get; private set; } = false;

    public virtual void Init(Action<PopupBase> onClose, params object[] args)
    {
        _onClose = onClose;
        if (!TryToGetArguments(args))
        {
            Debug.LogError("error when trying to get arguments!");
        }
    }

    public virtual void Close()
    {
        isMarkedToDelete = true;
        _onClose?.Invoke(this);
    }

    protected abstract bool TryToGetArguments(params object[] args);
}
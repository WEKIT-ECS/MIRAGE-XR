using System;
using UnityEngine;

public abstract class PopupBase : MonoBehaviour
{
    protected Action<PopupBase> _onClose;
    protected bool _canBeClosedByOutTap = false;
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
            Debug.LogError("error when trying to get arguments!");
        }
    }

    public virtual void Close()
    {
        _isMarkedToDelete = true;
        _onClose?.Invoke(this);
    }

    public Vector3 GetWorldPosition()
    {
        var rectTransform = (RectTransform)transform;
        var worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        return (worldCorners[0] + worldCorners[2]) / 2f;
    }

    protected abstract bool TryToGetArguments(params object[] args);
}

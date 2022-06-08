using UnityEngine;

public class BaseView : MonoBehaviour
{
    protected BaseView _parentView;

    public virtual void Initialization(BaseView parentView)
    {
        _parentView = parentView;
    }
}

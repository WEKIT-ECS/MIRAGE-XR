using UnityEngine;
using UnityEngine.Events;

public enum TrackingState
{
    Lost,
    Found,
    Limited
}

public abstract class ImageTargetBase : MonoBehaviour, IImageTarget
{
    protected ImageTargetModel _model;
    protected GameObject _targetObject;
    protected UnityEventImageTarget _onTargetFound = new UnityEventImageTarget();
    protected UnityEventImageTarget _onTargetLost = new UnityEventImageTarget();

    public string imageTargetName => _model.name;

    public GameObject targetObject => _targetObject;

    public UnityEventImageTarget onTargetFound => _onTargetFound;

    public UnityEventImageTarget onTargetLost => _onTargetLost;

    protected void OnStateChanged(TrackingState oldState, TrackingState newState)
    {
        bool isFound;

        if (_model.useLimitedTracking)
        {
            if (oldState == newState
                || (oldState == TrackingState.Found && newState == TrackingState.Limited)
                || (oldState == TrackingState.Limited && newState == TrackingState.Found))
            {
                return;
            }

            isFound = newState is TrackingState.Found or TrackingState.Limited;
        }
        else
        {
            if (oldState == newState
                || (oldState == TrackingState.Lost && newState == TrackingState.Limited)
                || (oldState == TrackingState.Limited && newState == TrackingState.Lost))
            {
                return;
            }

            isFound = newState is TrackingState.Found;
        }

        _targetObject.SetActive(isFound);
        if (isFound)
        {
            _onTargetFound.Invoke(this);
        }
        else
        {
            _onTargetLost.Invoke(this);
        }
    }

    public virtual void Initialization(ImageTargetModel model)
    {
        _model = model;
        _targetObject = Instantiate(model.prefab, transform);
        _targetObject.name = $"ImageTarget_{model.name}";
        _targetObject.SetActive(false);

        TrackerInitialization();
    }

    protected abstract void TrackerInitialization();
}

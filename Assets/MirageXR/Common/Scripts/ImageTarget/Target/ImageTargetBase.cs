using UnityEngine;

public enum TrackingState
{
    Lost,
    Found,
    Limited
}

public abstract class ImageTargetBase : MonoBehaviour
{
    protected ImageTargetModel _model;
    protected GameObject _targetObject;
    protected UnityEventImageTarget _onTargetFound = new UnityEventImageTarget();
    protected UnityEventImageTarget _onTargetLost = new UnityEventImageTarget();

    public UnityEventImageTarget onTargetFound => _onTargetFound;

    public UnityEventImageTarget onTargetLost => _onTargetLost;

    public string imageName => _model.name;

    protected void OnStateChanged(TrackingState oldState, TrackingState newState)
    {
        if (_model.useLimitedTracking)
        {
            if (oldState == newState
                || (oldState == TrackingState.Found && newState == TrackingState.Limited)
                || (oldState == TrackingState.Limited && newState == TrackingState.Found))
            {
                return;
            }

            _targetObject.SetActive(newState is TrackingState.Found or TrackingState.Limited);
            _onTargetFound.Invoke(this);
        }
        else
        {
            if (oldState == newState
                || (oldState == TrackingState.Lost && newState == TrackingState.Limited)
                || (oldState == TrackingState.Limited && newState == TrackingState.Lost))
            {
                return;
            }

            _targetObject.SetActive(newState is TrackingState.Found);
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

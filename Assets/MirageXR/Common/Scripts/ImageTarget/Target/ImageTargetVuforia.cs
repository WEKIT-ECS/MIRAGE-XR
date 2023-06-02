using System;
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ImageTargetBehaviour))]
public class ImageTargetVuforia : ImageTargetBase
{
    private ImageTargetBehaviour _imageTargetBehaviour;

    public ImageTargetBehaviour imageTargetBehaviour => _imageTargetBehaviour;

    private Status _oldTargetStatus;

    protected override void TrackerInitialization()
    {
        _imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
        _oldTargetStatus = _imageTargetBehaviour.TargetStatus.Status;
        _imageTargetBehaviour.OnTargetStatusChanged += OnTrackableStatusChangedAction;
        Debug.Log("TrackerInitialization for " + _imageTargetBehaviour.TargetName + " Complete");
    }

    private void OnTrackableStatusChangedAction(ObserverBehaviour observerBehaviour, TargetStatus targetStatusChangeResult)
    {
        var statusChangeResult = targetStatusChangeResult.Status;
        //Debug.Log(_imageTargetBehaviour.TargetName + " Tracking status changed to " + statusChangeResult.ToString() + " From " + _oldTargetStatus.ToString());
        OnStateChanged(ToTrackingState(_oldTargetStatus), ToTrackingState(statusChangeResult));
        _oldTargetStatus = statusChangeResult;
    }



    private static TrackingState ToTrackingState(Status state)
    {
        return state switch
        {
            Status.NO_POSE => TrackingState.Lost,
            Status.LIMITED => TrackingState.Limited,
            Status.EXTENDED_TRACKED => TrackingState.Limited,
            Status.TRACKED => TrackingState.Found,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

    }
}

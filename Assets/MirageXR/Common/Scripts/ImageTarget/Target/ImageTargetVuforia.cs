using System;
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ImageTargetBehaviour))]
public class ImageTargetVuforia : ImageTargetBase
{
    private ImageTargetBehaviour _imageTargetBehaviour;

    public ImageTargetBehaviour imageTargetBehaviour => _imageTargetBehaviour;

    protected override void TrackerInitialization()
    {
        _imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
        _imageTargetBehaviour.RegisterOnTrackableStatusChanged(OnTrackableStatusChangedAction);
    }

    private void OnTrackableStatusChangedAction(TrackableBehaviour.StatusChangeResult statusChangeResult)
    {
        OnStateChanged(ToTrackingState(statusChangeResult.PreviousStatus), ToTrackingState(statusChangeResult.NewStatus));
    }

    private static TrackingState ToTrackingState(TrackableBehaviour.Status state)
    {
        return state switch
        {
            TrackableBehaviour.Status.NO_POSE => TrackingState.Lost,
            TrackableBehaviour.Status.DETECTED => TrackingState.Lost,
            TrackableBehaviour.Status.LIMITED => TrackingState.Limited,
            TrackableBehaviour.Status.EXTENDED_TRACKED => TrackingState.Limited,
            TrackableBehaviour.Status.TRACKED => TrackingState.Found,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}

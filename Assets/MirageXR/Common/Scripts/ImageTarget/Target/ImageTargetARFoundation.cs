using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ARTrackingState = UnityEngine.XR.ARSubsystems.TrackingState;

[RequireComponent(typeof(ARTrackedImage))]
public class ImageTargetARFoundation : ImageTargetBase
{
    private ARTrackedImage _image;
    private ARTrackingState _state = ARTrackingState.None;

    private void Update()
    {
        CheckTrackingState();
    }

    private void CheckTrackingState()
    {
        if (!_image)
        {
            return;
        }

        if (_state != _image.trackingState)
        {
            OnStateChanged(ToTrackingState(_state), ToTrackingState(_image.trackingState));
            _state = _image.trackingState;
        }
    }

    protected override void TrackerInitialization()
    {
        _image = GetComponent<ARTrackedImage>();
    }

    private static TrackingState ToTrackingState(ARTrackingState state)
    {
        return state switch
        {
            ARTrackingState.None => TrackingState.Lost,
            ARTrackingState.Limited => TrackingState.Limited,
            ARTrackingState.Tracking => TrackingState.Found,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}

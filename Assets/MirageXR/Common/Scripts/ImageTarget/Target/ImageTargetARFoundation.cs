using System;
using UnityEngine.XR.ARFoundation;
using ARTrackingState = UnityEngine.XR.ARSubsystems.TrackingState;

public class ImageTargetARFoundation : ImageTargetBase
{
    private ARTrackedImage _image;
    private ARTrackingState _state = ARTrackingState.None;

    public void SetARTrackedImage(ARTrackedImage trackedImage)
    {
        _image = trackedImage;
        CopyPose(trackedImage);
    }

    public void RemoveARTrackedImage()
    {
        _image = null;
    }

    public void CopyPose(ARTrackedImage trackedImage)
    {
        var trackedImageTransform = trackedImage.transform;
        transform.position = trackedImageTransform.position;
        transform.rotation = trackedImageTransform.rotation;
        transform.localScale = trackedImageTransform.localScale;
    }

    private void Update()
    {
        CheckTrackingState();
    }

    private void CheckTrackingState()
    {
        if (!_image)
        {
            OnStateChanged(ToTrackingState(_state), ToTrackingState(ARTrackingState.None));
            _state = ARTrackingState.None;
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
        _image = null;
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

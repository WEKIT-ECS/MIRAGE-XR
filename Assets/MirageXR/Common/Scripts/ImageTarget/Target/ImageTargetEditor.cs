using UnityEngine;

public class ImageTargetEditor : ImageTargetBase
{
    private const float ADDITIONAL_SACLE = 0.1f;

    private bool _isCenterInViewPort;

    protected override void TrackerInitialization()
    {
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(transform);
        var aspect = (float)_model.texture2D.height / _model.texture2D.width;
        plane.transform.localScale = new Vector3(_model.width * ADDITIONAL_SACLE, _model.width * ADDITIONAL_SACLE, _model.width * aspect * ADDITIONAL_SACLE);
        plane.transform.localPosition = Vector3.zero;
        var material = plane.GetComponentInChildren<MeshRenderer>().material;
        material.mainTexture = _model.texture2D;
    }

    private void Update()
    {
        var isCenterInViewPortNewValue = IsCenterInViewPort();
        if (_isCenterInViewPort != isCenterInViewPortNewValue)
        {
            OnStateChanged(ToTrackingState(_isCenterInViewPort), ToTrackingState(isCenterInViewPortNewValue));
            _isCenterInViewPort = isCenterInViewPortNewValue;
        }
    }

    private bool IsCenterInViewPort()
    {
        var viewCamera = Camera.main;
        if (!viewCamera)
        {
            return false;
        }

        var screenPoint = viewCamera.WorldToViewportPoint(transform.position);
        return screenPoint.z > 0 && screenPoint.x is > 0 and < 1 && screenPoint.y is > 0 and < 1;
    }

    private TrackingState ToTrackingState(bool state)
    {
        return state ? TrackingState.Found : _targetObject.activeSelf ? TrackingState.Limited : TrackingState.Lost;
    }
}

using System.Collections;
using MirageXR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CameraCalibrationChecker : MonoBehaviour
{
    public class UnityEventFloat : UnityEvent<float> { }

    private const float COOLDOWN = 1.0f;
    private const float ADDITIONAL_COOLDOWN = 4.0f;
    private const float MAX_AVALIBLE_DELTA_DISTANCE = 2.0f;

    public UnityEventFloat OnAnchorLost => _onAnchorLost;

    private Transform _mainCameraTransform;
    private Transform _anchor;
    private IViewManager _viewManager;
    private float _distance;
    private Coroutine _coroutine;
    private readonly UnityEventFloat _onAnchorLost = new();
    private bool _isWorking;
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
    private ARSession _arSession;
    private XRSessionSubsystem _subsystem;
    private UnityEngine.XR.ARSubsystems.TrackingState _oldTrackingState = UnityEngine.XR.ARSubsystems.TrackingState.Tracking;
#endif

    public void Initialization(IViewManager viewManager)
    {
        UnityEngine.Debug.Log("Initializing [CameraCalibrationChecker] <--");
        _viewManager = viewManager;
        _anchor = RootObject.Instance.CalibrationManager.Anchor;
        _mainCameraTransform = _viewManager.GetCamera().transform;
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
        _arSession = MirageXR.Utilities.FindOrCreateComponent<ARSession>();
#endif
        UnityEngine.Debug.Log("Initializing [CameraCalibrationChecker] -->");
    }

    public void RunChecker()
    {
        if (!_isWorking)
        {
            _isWorking = true;
            _coroutine = StartCoroutine(CalibrationCheckerCoroutine());
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
            _subsystem = _arSession.subsystem;
#endif
        }
    }

    public void StopChecker()
    {
        _isWorking = false;
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    private IEnumerator CalibrationCheckerCoroutine()
    {
        _distance = Vector3.Distance(_mainCameraTransform.position, _anchor.position);

        while (_isWorking)
        {
            var newDistance = Vector3.Distance(_mainCameraTransform.position, _anchor.position);
            var isDistanceLost = Mathf.Abs(_distance - newDistance) > MAX_AVALIBLE_DELTA_DISTANCE;
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
            var trackingState = UnityEngine.XR.ARSubsystems.TrackingState.Tracking;
            if (_subsystem != null)
            {
                trackingState = _subsystem.trackingState;
            }

            if (isDistanceLost || (trackingState != _oldTrackingState && trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None))
#else
            if (isDistanceLost)
#endif
            {
                _onAnchorLost?.Invoke(_distance);
                yield return new WaitForSeconds(ADDITIONAL_COOLDOWN);
                _distance = Vector3.Distance(_mainCameraTransform.position, _anchor.position);
            }
#if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
            _oldTrackingState = trackingState;
#endif
            _distance = newDistance;

            yield return new WaitForSeconds(COOLDOWN);
        }
    }
}

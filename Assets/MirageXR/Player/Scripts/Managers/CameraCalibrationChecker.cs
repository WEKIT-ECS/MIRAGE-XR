using System.Collections;
using MirageXR;
using UnityEngine;
using UnityEngine.Events;

public class CameraCalibrationChecker : MonoBehaviour
{
    public class UnityEventFloat : UnityEvent<float> { }

    private const float COOLDOWN = 1.0f;
    private const float ADDITIONAL_COOLDOWN = 4.0f;
    private const float MAX_AVALIBLE_DELTA_DISTANCE = 2.0f;

    [SerializeField] private Transform _mainCameraTransform;

    public UnityEventFloat onAnchorLost => _onAnchorLost;

    private Transform _anchor;
    private float _distance;
    private Coroutine _coroutine;
    private UnityEventFloat _onAnchorLost = new UnityEventFloat();
    private bool _isWorking;

    public void Initialization()
    {
        var calibrationManager = RootObject.Instance.calibrationManager;
        _anchor = RootObject.Instance.calibrationManager.anchor;
        calibrationManager.onCalibrationStarted.AddListener(StopChecker);
        calibrationManager.onCalibrationFinished.AddListener(RunChecker);
        calibrationManager.onCalibrationCanceled.AddListener(RunChecker);
    }

    public void RunChecker()
    {
        if (!_isWorking)
        {
            _isWorking = true;
            _coroutine = StartCoroutine(CalibrationCheckerCoroutine());
            UnityEngine.Debug.Log(_coroutine);
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
            if (Mathf.Abs(_distance - newDistance) > MAX_AVALIBLE_DELTA_DISTANCE)
            {
                _onAnchorLost?.Invoke(_distance);
                yield return new WaitForSeconds(ADDITIONAL_COOLDOWN);
                _distance = Vector3.Distance(_mainCameraTransform.position, _anchor.position);
            }

            _distance = newDistance;

            yield return new WaitForSeconds(COOLDOWN);
        }
    }
}

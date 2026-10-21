using System;
using System.Collections;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FloorManagerARFoundation : FloorManagerBase
{
    private const float _DEFAULT_FLOOR_LEVEL = -1.6f;
    private const float MIN_HEAD_HEIGHT = 0.5f;
    private const float MAX_HEAD_HEIGHT = 2.3f;
    private const float DETECTION_INTERVAL = 1.2f;

    public override event Action<float> OnFloorLevelChanged;

    private GameObject _prefabAnchor;
    private ARAnchorManager _arAnchorManager;
    private ARPlaneManager _arPlaneManager;
    private ARSession _arSession;
    private PlaneId _planeId = PlaneId.InvalidId;
    private Vector3 _floorLevel = new Vector3(0, _DEFAULT_FLOOR_LEVEL, 0);
    private GameObject _floorPlane;
    private IViewManager _viewManager;
    private PlaneManagerWrapper _planeManager;
    private bool _isManualFloor = false;
    private bool _isFloorDetected = false;
    private Coroutine _detectFloorCoroutine;

    public override float floorLevel
    {
        get
        {
            if (_floorPlane != null)
            {
                _floorLevel = _floorPlane.transform.position;
            }

            return _floorLevel.y;
        }
    }

    public GameObject prefabAnchor
    {
        get => _prefabAnchor;
        set => _prefabAnchor = value;
    }

    public override bool isFloorDetected => _isFloorDetected;

    public override bool isManualFloor => _isManualFloor;

    public override async Task<bool> InitializationAsync(IViewManager viewManager, PlaneManagerWrapper planeManager)
    {
        _viewManager = viewManager;
        _planeManager = planeManager;
        _arSession = MirageXR.Utilities.FindOrCreateComponent<ARSession>(_viewManager.BaseCamera.gameObject);
        _arAnchorManager = MirageXR.Utilities.FindOrCreateComponent<ARAnchorManager>(_viewManager.BaseCamera.gameObject);
        _arPlaneManager = MirageXR.Utilities.FindOrCreateComponent<ARPlaneManager>(_viewManager.BaseCamera.gameObject);

        _planeManager.onPlaneRemoved.AddListener(OnPlaneRemoved);

        if (_detectFloorCoroutine != null)
        {
            StopCoroutine(_detectFloorCoroutine);
        }
        _detectFloorCoroutine = StartCoroutine(DetectFloorRoutine());

        await Task.Yield();

        return true;
    }

    private void OnDestroy()
    {
        if (_detectFloorCoroutine != null)
        {
            StopCoroutine(_detectFloorCoroutine);
            _detectFloorCoroutine = null;
        }

        if (_planeManager != null)
        {
            _planeManager.onPlaneRemoved.RemoveListener(OnPlaneRemoved);
        }
    }

    private IEnumerator DetectFloorRoutine()
    {
        var wait = new WaitForSeconds(DETECTION_INTERVAL);

        while (true)
        {
            yield return wait;

            if (_isManualFloor)
            {
                if (_floorPlane != null)
                {
                    _floorLevel = _floorPlane.transform.position;
                }
                continue;
            }

            Transform camTransform = _viewManager?.BaseCamera != null ? _viewManager.BaseCamera.transform : (Camera.main != null ? Camera.main.transform : null);
            if (camTransform == null)
            {
                continue;
            }

            Vector3 camPos = camTransform.position;
            ARPlane bestPlane = null;
            bool foundClassifiedFloor = false;
            float lowestY = float.MaxValue;
            float classifiedFloorY = float.MaxValue;

            if (_arPlaneManager != null)
            {
                foreach (var plane in _arPlaneManager.trackables)
                {
                    if (plane == null || !plane.gameObject.activeInHierarchy) continue;
                    if (plane.alignment != PlaneAlignment.HorizontalUp) continue;

                    float planeY = plane.transform.position.y;
                    float heightDiff = camPos.y - planeY;

                    if (heightDiff >= MIN_HEAD_HEIGHT && heightDiff <= MAX_HEAD_HEIGHT)
                    {
                        if (plane.classification == PlaneClassification.Floor)
                        {
                            if (!foundClassifiedFloor || planeY < classifiedFloorY)
                            {
                                foundClassifiedFloor = true;
                                bestPlane = plane;
                                classifiedFloorY = planeY;
                            }
                        }
                        else if (!foundClassifiedFloor)
                        {
                            if (planeY < lowestY)
                            {
                                lowestY = planeY;
                                bestPlane = plane;
                            }
                        }
                    }
                }
            }

            if (bestPlane != null)
            {
                float newY = bestPlane.transform.position.y;
                var trackableId = bestPlane.trackableId;
                _planeId = new PlaneId(trackableId.subId1, trackableId.subId2);
                _floorPlane = bestPlane.gameObject;

                if (!_isFloorDetected || Mathf.Abs(newY - _floorLevel.y) > 0.03f)
                {
                    _floorLevel = new Vector3(bestPlane.transform.position.x, newY, bestPlane.transform.position.z);
                    _isFloorDetected = true;
                    OnFloorLevelChanged?.Invoke(_floorLevel.y);
                }
            }
            else
            {
                // Fallback 1: Downward raycast against spatial mesh
                if (Physics.Raycast(new Ray(camPos, Vector3.down), out RaycastHit hit, MAX_HEAD_HEIGHT))
                {
                    float hitDiff = camPos.y - hit.point.y;
                    if (hitDiff >= MIN_HEAD_HEIGHT)
                    {
                        if (!_isFloorDetected || Mathf.Abs(hit.point.y - _floorLevel.y) > 0.05f)
                        {
                            _floorLevel = hit.point;
                            _isFloorDetected = true;
                            OnFloorLevelChanged?.Invoke(_floorLevel.y);
                        }
                    }
                }
                else if (!_isFloorDetected)
                {
                    // Fallback 2: Camera eye-height offset (-1.6m)
                    _floorLevel = new Vector3(camPos.x, camPos.y - 1.6f, camPos.z);
                    _isFloorDetected = true;
                    OnFloorLevelChanged?.Invoke(_floorLevel.y);
                }
            }
        }
    }

    private void OnPlaneRemoved(PlaneId planeId, PlaneId subsumedByPlaneId)
    {
        if (_planeId != planeId)
        {
            return;
        }

        var plane = RootObject.Instance.PlaneManager.GetPlane(subsumedByPlaneId);
        if (plane != null)
        {
            _planeId = subsumedByPlaneId;
            _floorPlane = plane;
            return;
        }

        _planeId = PlaneId.InvalidId;
        _floorPlane = null;
        if (_isManualFloor)
        {
            _isManualFloor = false;
        }

        RootObject.Instance.PlaneManager.UpdatePlanes();
    }

    public override async Task<bool> ResetAsync()
    {
        if (_detectFloorCoroutine != null)
        {
            StopCoroutine(_detectFloorCoroutine);
            _detectFloorCoroutine = null;
        }

        if (!_arSession)
        {
            Debug.LogError("ARSession is null");
            return false;
        }

        if (!_arAnchorManager)
        {
            Debug.LogError("ARAnchorManager is null");
            return false;
        }

        Destroy(_arAnchorManager);

        _planeId = PlaneId.InvalidId;
        _isManualFloor = false;
        _isFloorDetected = false;
        _floorPlane = null;

        await Task.Yield();
        await InitializationAsync(_viewManager, _planeManager);

        return true;
    }

    public override Transform CreateAnchor(Pose pose)
    {
        if (_planeId != PlaneId.InvalidId)
        {
            var planeGameObject = RootObject.Instance.PlaneManager.GetPlane(_planeId);
            var plane = planeGameObject ? planeGameObject.GetComponent<ARPlane>() : null;
            if (plane && _arAnchorManager)
            {
                var oldPrefab = _arAnchorManager.anchorPrefab;
                _arAnchorManager.anchorPrefab = _prefabAnchor;
                var attachedAnchor = _arAnchorManager.AttachAnchor(plane, pose);
                _arAnchorManager.anchorPrefab = oldPrefab;

                if (attachedAnchor)
                {
                    return attachedAnchor.transform;
                }
            }
        }

        // Standalone world anchor when no floor plane is detected or attachment is unavailable
        GameObject anchorObj;
        if (_prefabAnchor != null)
        {
            anchorObj = Instantiate(_prefabAnchor, pose.position, pose.rotation);
        }
        else
        {
            anchorObj = new GameObject("ARAnchor");
            anchorObj.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        return anchorObj.transform;
    }

    public override void SetFloor(PlaneId planeId, Vector3 position)
    {
        _isManualFloor = true;
        _planeId = planeId;
        _floorLevel = position;
        _isFloorDetected = true;
        var planeGameObject = RootObject.Instance.PlaneManager.GetPlane(_planeId);
        if (planeGameObject == null)
        {
            Debug.LogError($"Can't find GameObject with id: {_planeId}");
        }

        _floorPlane = planeGameObject;
        OnFloorLevelChanged?.Invoke(_floorLevel.y);
    }
}

using System;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FloorManager : MonoBehaviour
{
    [Serializable]
    public enum ForceManagerType
    {
        Default,
        Editor,
        ARFoundation,
        MRTK,
    }

    private const PlaneDetectionMode DETECTION_MODE = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;

    [SerializeField] private ForceManagerType _forceManagerType = ForceManagerType.Default;
    [SerializeField] private GameObject _prefabAnchor;
    [SerializeField] private GameObject _prefabPlace;
    [SerializeField] private PlaneBehaviour _prefabDebugPlane;

    private ARPlaneManager _arPlaneManager;
    private ARAnchorManager _arAnchorManager;
    private ARSession _arSession;
    private TrackableId _floorId = TrackableId.invalidId;
    private bool _enableColliders;
    private bool _showPlanes;
    private Action<TrackableId> _onFloorDetected;
    private PlaneBehaviour _debugPlane;

    public bool enableColliders => _enableColliders;

    public bool showPlanes => _showPlanes;

    public TrackableId floorId => _floorId;

    public async Task<bool> InitializationAsync()
    {
        if (_forceManagerType == ForceManagerType.Default) // TODO: create wrapper
        {
#if UNITY_EDITOR
            _forceManagerType = ForceManagerType.Editor;
#elif UNITY_ANDROID || UNITY_IOS
            _forceManagerType = ForceManagerType.ARFoundation;
#else
            _forceManagerType = ForceManagerType.MRTK;
#endif
        }

        var mainCamera = Camera.main;

        if (!mainCamera)
        {
            Debug.LogError("Can't find camera main");
            return false;
        }

        var cameraParent = mainCamera.transform.parent ? mainCamera.transform.parent.gameObject : mainCamera.gameObject;

        _arSession = Utilities.FindOrCreateComponent<ARSession>(cameraParent);
        _arPlaneManager = Utilities.FindOrCreateComponent<ARPlaneManager>(cameraParent);
        _arAnchorManager = Utilities.FindOrCreateComponent<ARAnchorManager>(cameraParent);

        _arPlaneManager.requestedDetectionMode = DETECTION_MODE;
        _arPlaneManager.planePrefab = _prefabPlace;

        await Task.Yield();

        _enableColliders = false;
        _showPlanes = false;

        EventManager.OnEditModeChanged += OnEditModeChanged;

        return true;
    }

    public async Task<bool> ResetAsync()
    {
        if (!_arSession)
        {
            Debug.LogError("ARSession is null");
            return false;
        }

        if (!_arPlaneManager)
        {
            Debug.LogError("ARPlaneManager is null");
            return false;
        }

        if (!_arAnchorManager)
        {
            Debug.LogError("ARAnchorManager is null");
            return false;
        }

        _arPlaneManager.enabled = false;
        _arAnchorManager.enabled = false;

        _arSession.Reset();
        await Task.Yield();

        _arPlaneManager.enabled = true;
        _arAnchorManager.enabled = true;

        return true;
    }

    public ARAnchor CreateAnchor(Pose pose)
    {
        if (_floorId == TrackableId.invalidId)
        {
            Debug.LogError("Floor is not detected");
            return null;
        }

        var oldPrefab = _arAnchorManager.anchorPrefab;
        _arAnchorManager.anchorPrefab = _prefabAnchor;

        var plane = _forceManagerType == ForceManagerType.Editor
            ? _debugPlane.GetComponent<ARPlane>()
            : _arPlaneManager.GetPlane(_floorId);

        if (!plane)
        {
            Debug.LogError($"Can't find plane with id: {_floorId}");
            return null;
        }

        ARAnchor anchor;
        if (_forceManagerType == ForceManagerType.Editor)
        {
            var anchorObj = Instantiate(_prefabAnchor, pose.position, pose.rotation);
            anchor = anchorObj.AddComponent<ARAnchor>();
            anchor.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
        else
        {
            anchor = _arAnchorManager.AttachAnchor(plane, pose);
        }

        _arAnchorManager.anchorPrefab = oldPrefab;

        return anchor;
    }

    public void SetFloor(PlaneBehaviour floor)
    {
        _floorId = floor.trackableId;

        var render = floor.GetComponent<MeshRenderer>();
        if (!render)
        {
            Debug.LogError("Can't find MeshRenderer");
            return;
        }

        _onFloorDetected?.Invoke(_floorId);
        UpdatePlanes();
    }

    public void EnableFloorDetection(Action<TrackableId> onFloorDetected)
    {
        if (_enableColliders)
        {
            return;
        }

        if (_forceManagerType == ForceManagerType.Editor)
        {
            if (!_debugPlane)
            {
                _debugPlane = Instantiate(_prefabDebugPlane);
            }
        }

        _enableColliders = true;
        _onFloorDetected = onFloorDetected;

        UpdatePlanes();
    }

    public void DisableFloorDetection()
    {
        if (!_enableColliders)
        {
            return;
        }

        _onFloorDetected = null;
        _enableColliders = false;
        UpdatePlanes();
    }

    public void Unsubscribe()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool value)
    {
        _showPlanes = value;
        UpdatePlanes();
    }

    private void UpdatePlanes()
    {
        foreach (var plane in _arPlaneManager.trackables)
        {
            var planeBehaviour = plane.GetComponent<PlaneBehaviour>();
            if (planeBehaviour)
            {
                planeBehaviour.UpdateState();
            }
        }

        if (_forceManagerType == ForceManagerType.Editor)
        {
            if (_debugPlane)
            {
                _debugPlane.UpdateState();
            }
        }
    }
}

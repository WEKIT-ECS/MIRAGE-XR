using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Action = System.Action;

public class FloorManagerARFoundation : FloorManagerBase
{
    private const float _DEFAULT_FLOOR_LEVEL = -1f;
    private const PlaneDetectionMode DETECTION_MODE = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;

    private GameObject _prefabAnchor;
    private GameObject _prefabPlane;
    private ARPlaneManager _arPlaneManager;
    private ARAnchorManager _arAnchorManager;
    private ARSession _arSession;
    private TrackableId _floorId = TrackableId.invalidId;
    private bool _enableColliders;
    private bool _showPlanes;
    private ARFoundationPlaneBehaviour _debugARFoundationPlane;
    private Vector3 _floorLevel;
    private ARPlane _floorPlane;

    public override bool enableColliders => _enableColliders;

    public override bool showPlanes => _showPlanes;

    public override float floorLevel
    {
        get
        {
            if (!isFloorDetected)
            {
                return _DEFAULT_FLOOR_LEVEL;
            }

            //if (!_floorPlane)
            //{
            //    _floorPlane = _arPlaneManager.GetPlane(_floorId);
            //}

            if (_floorPlane)
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

    public GameObject prefabPlane
    {
        get => _prefabPlane;
        set => _prefabPlane = value;
    }

    public TrackableId floorId => _floorId;

    public override bool isFloorDetected => floorId != TrackableId.invalidId;

    public override void Dispose()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    public override async Task<bool> InitializationAsync()
    {
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
        _arPlaneManager.planePrefab = _prefabPlane;

        await Task.Yield();

        _enableColliders = false;
        _showPlanes = false;

        EventManager.OnEditModeChanged += OnEditModeChanged;

        return true;
    }

    public override async Task<bool> ResetAsync()
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

        foreach (var arPlane in _arPlaneManager.trackables)
        {
            Destroy(arPlane.gameObject);
        }

        Destroy(_arPlaneManager);
        Destroy(_arAnchorManager);

        _floorId = TrackableId.invalidId;
        _onFloorDetected = null;

        await Task.Yield();

        await InitializationAsync();

        return true;
    }

    public override Transform CreateAnchor(Pose pose)
    {
        if (_floorId == TrackableId.invalidId)
        {
            Debug.LogError("Floor is not detected");
            return null;
        }

        var oldPrefab = _arAnchorManager.anchorPrefab;
        _arAnchorManager.anchorPrefab = _prefabAnchor;

        var plane = _arPlaneManager.GetPlane(_floorId);

        if (!plane)
        {
            Debug.LogError($"Can't find plane with id: {_floorId}");
            return null;
        }

        var anchor = _arAnchorManager.AttachAnchor(plane, pose);

        _arAnchorManager.anchorPrefab = oldPrefab;

        return anchor.transform;
    }

    public override void SetFloor(IPlaneBehaviour floor)
    {
        var arFloor = floor as ARFoundationPlaneBehaviour;
        _floorId = arFloor.trackableId;

        _floorLevel = arFloor.transform.position;
        _floorPlane = _arPlaneManager.GetPlane(_floorId);

        _onFloorDetected?.Invoke();
        UpdatePlanes();
    }

    public override void EnableFloorDetection(Action onFloorDetected)
    {
        if (_enableColliders)
        {
            return;
        }

        _enableColliders = true;
        _onFloorDetected = onFloorDetected;

        UpdatePlanes();
    }

    public override void DisableFloorDetection()
    {
        if (!_enableColliders)
        {
            return;
        }

        _onFloorDetected = null;
        _enableColliders = false;
        UpdatePlanes();
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
            var planeBehaviour = plane.GetComponent<ARFoundationPlaneBehaviour>();
            if (planeBehaviour)
            {
                planeBehaviour.UpdateState();
            }
        }
    }
}

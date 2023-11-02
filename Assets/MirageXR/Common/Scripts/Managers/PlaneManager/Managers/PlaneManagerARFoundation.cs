using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneManagerARFoundation : PlaneManagerBase
{
    private const PlaneDetectionMode DETECTION_MODE = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;

    public override UnityEventPlaneIdVector3 onPlaneClicked => _onPlaneClicked;

    private GameObject _prefabPlane;
    private ARPlaneManager _arPlaneManager;
    private ARSession _arSession;
    private bool _enableColliders;
    private bool _showPlanes;
    private ARFoundationPlaneBehaviour _debugARFoundationPlane;
    private UnityEventPlaneIdVector3 _onPlaneClicked = new UnityEventPlaneIdVector3();

    public GameObject prefabPlane
    {
        get => _prefabPlane;
        set => _prefabPlane = value;
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

        foreach (var arPlane in _arPlaneManager.trackables)
        {
            Destroy(arPlane.gameObject);
        }

        Destroy(_arPlaneManager);


        await Task.Yield();
        await InitializationAsync();

        return true;
    }

    public override void OnPlaneClicked(PlaneId planeId, Vector3 position)
    {
        _onPlaneClicked.Invoke(planeId, position);
    }

    public override void EnablePlanes()
    {
        if (_enableColliders)
        {
            return;
        }

        _enableColliders = true;
        _showPlanes = true;

        UpdatePlanes();
    }

    public override void DisablePlanes()
    {
        if (!_enableColliders)
        {
            return;
        }

        _enableColliders = false;
        _showPlanes = RootObject.Instance.activityManager.EditModeActive;
        UpdatePlanes();
    }

    public override void SelectPlane(PlaneId planeId)
    {
        ARFoundationPlaneBehaviour.SetSelectedPlane(planeId);
        UpdatePlanes();
    }

    public override GameObject GetPlane(PlaneId planeId)
    {
        var plane = _arPlaneManager.GetPlane(new TrackableId(planeId.subId1, planeId.subId2));
        if (plane == null)
        {
            Debug.LogError($"Can't find plane with id: {planeId}");
            return null;
        }

        return plane.gameObject;
    }

    public override void UpdatePlanes()
    {
        ARFoundationPlaneBehaviour.UpdatePlanesState(_showPlanes, _enableColliders, OnPlaneClicked);

        foreach (var plane in _arPlaneManager.trackables)
        {
            var planeBehaviour = plane.GetComponent<ARFoundationPlaneBehaviour>();
            if (planeBehaviour)
            {
                planeBehaviour.UpdateState();
            }
        }
    }

    public override void Dispose()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool value)
    {
        _showPlanes = value || _enableColliders;
        UpdatePlanes();
    }
}

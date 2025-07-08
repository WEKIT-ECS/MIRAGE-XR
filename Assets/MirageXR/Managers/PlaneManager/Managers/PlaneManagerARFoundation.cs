using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneManagerARFoundation : PlaneManagerBase
{
    private const PlaneDetectionMode DETECTION_MODE = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;

    public override UnityEventPlaneIdVector3 onPlaneClicked => _onPlaneClicked;
    public override UnityEventPlaneIdPlaneId onPlaneRemoved => _onPlaneRemoved;

    private GameObject _prefabPlane;
    private ARPlaneManager _arPlaneManager;
    private ARSession _arSession;
    private bool _enableColliders;
    private bool _showPlanes;
    private ARFoundationPlaneBehaviour _debugARFoundationPlane;
    private IViewManager _viewManager;
    private readonly UnityEventPlaneIdVector3 _onPlaneClicked = new();
    private readonly UnityEventPlaneIdPlaneId _onPlaneRemoved = new();

    public GameObject prefabPlane
    {
        get => _prefabPlane;
        set => _prefabPlane = value;
    }

    public override async Task<bool> InitializationAsync(IViewManager viewManager)
    {
        _viewManager = viewManager;
        var mainCamera = RootObject.Instance.BaseCamera;

        if (!mainCamera)
        {
            Debug.LogError("Can't find camera main");
            return false;
        }

        _arSession = MirageXR.Utilities.FindOrCreateComponent<ARSession>(_viewManager.BaseCamera.gameObject);
        _arPlaneManager = MirageXR.Utilities.FindOrCreateComponent<ARPlaneManager>(_viewManager.BaseCamera.gameObject);

        _arPlaneManager.requestedDetectionMode = DETECTION_MODE;
        _arPlaneManager.planePrefab = _prefabPlane;

        await Task.Yield();

        _enableColliders = false;
        _showPlanes = false;

        _arPlaneManager.trackablesChanged.AddListener(ArPlaneManagerOnPlanesChanged);

        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;

        return true;
    }
    
    private void ArPlaneManagerOnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        foreach (var (id, arPlane) in eventArgs.removed)
        {
            if (arPlane.subsumedBy == null)
            {
                _onPlaneRemoved.Invoke(new PlaneId(id.subId1, id.subId2), PlaneId.InvalidId);
            }
            else
            {
                var subsumedById = arPlane.subsumedBy.trackableId;
                _onPlaneRemoved.Invoke(new PlaneId(id.subId1, id.subId2), new PlaneId(subsumedById.subId1, subsumedById.subId2));
            }
        }
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
        await InitializationAsync(_viewManager);

        return true;
    }

    public override void OnPlaneClicked(PlaneId planeId, Vector3 position)
    {
        var plane = _arPlaneManager.GetPlane(new TrackableId(planeId.subId1, planeId.subId2));
        if (plane != null)
        {
            _onPlaneClicked.Invoke(planeId, position);
        }
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
        _showPlanes = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive;
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
        return plane == null ? null : plane.gameObject;
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

    public override GameObject GetRandomPlane()
    {
        foreach (var arPlane in _arPlaneManager.trackables)
        {
            if (arPlane != null)
            {
                return arPlane.gameObject;
            }
        }

        return null;
    }

    public override void Dispose()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void OnEditModeChanged(bool value)
    {
        _showPlanes = value || _enableColliders;
        UpdatePlanes();
    }
}

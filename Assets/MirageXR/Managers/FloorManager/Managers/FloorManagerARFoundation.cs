using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FloorManagerARFoundation : FloorManagerBase
{
    private const float _DEFAULT_FLOOR_LEVEL = -1f;

    private GameObject _prefabAnchor;
    private ARAnchorManager _arAnchorManager;
    private ARSession _arSession;
    private PlaneId _planeId = PlaneId.InvalidId;
    private Vector3 _floorLevel;
    private GameObject _floorPlane;
    private IViewManager _viewManager;
    private PlaneManagerWrapper _planeManager;

    public override float floorLevel
    {
        get
        {
            if (!isFloorDetected)
            {
                return _DEFAULT_FLOOR_LEVEL;
            }

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

    public override bool isFloorDetected => _planeId != PlaneId.InvalidId;

    public override async Task<bool> InitializationAsync(IViewManager viewManager, PlaneManagerWrapper planeManager)
    {
        _viewManager = viewManager;
        _planeManager = planeManager;
        _arSession = MirageXR.Utilities.FindOrCreateComponent<ARSession>(_viewManager.BaseCamera.gameObject);
        _arAnchorManager = MirageXR.Utilities.FindOrCreateComponent<ARAnchorManager>(_viewManager.BaseCamera.gameObject);

        _planeManager.onPlaneRemoved.AddListener(OnPlaneRemoved);
        
        await Task.Yield();

        return true;
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
            return;
        }

        plane = RootObject.Instance.PlaneManager.GetRandomPlane();
        if (plane)
        {
            var arPlane = plane.GetComponent<ARPlane>();
            if (arPlane)
            {
                var id = arPlane.trackableId;
                _planeId = new PlaneId(id.subId1, id.subId2);
            }
        }

        RootObject.Instance.PlaneManager.UpdatePlanes();
    }

    public override async Task<bool> ResetAsync()
    {
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

        if (_arAnchorManager != null && anchorObj.GetComponent<ARAnchor>() == null)
        {
            anchorObj.AddComponent<ARAnchor>();
        }

        return anchorObj.transform;
    }

    public override void SetFloor(PlaneId planeId, Vector3 position)
    {
        _planeId = planeId;
        _floorLevel = position;
        var planeGameObject = RootObject.Instance.PlaneManager.GetPlane(_planeId);
        if (planeGameObject == null)
        {
            Debug.LogError($"Can't find GameObject with id: {_planeId}");
        }

        _floorPlane = planeGameObject;
    }
}

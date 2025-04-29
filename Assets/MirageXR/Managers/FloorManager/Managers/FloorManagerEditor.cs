using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public class FloorManagerEditor : FloorManagerBase
{
    private GameObject _prefabAnchor;
    private EditorPlaneBehaviour _plane;
    private Vector3 _floorLevel;
    private bool _enableColliders;
    private PlaneId _planeId;
    private GameObject _anchor;

    public override float floorLevel => _floorLevel.y;

    public override bool isFloorDetected => _planeId != PlaneId.InvalidId;

    public GameObject prefabAnchor
    {
        get => _prefabAnchor;
        set => _prefabAnchor = value;
    }

    public override Task<bool> InitializationAsync(IViewManager viewManager, PlaneManagerWrapper planeManager)
    {
        _planeId = PlaneId.InvalidId;
        _floorLevel = new Vector3(0, _DEFAULT_FLOOR_LEVEL, 0);
        return Task.FromResult(true);
    }

    public override Task<bool> ResetAsync()
    {
        _floorLevel = new Vector3(0, _DEFAULT_FLOOR_LEVEL, 0);
        _planeId = PlaneId.InvalidId;
        return Task.FromResult(true);
    }

    public override Transform CreateAnchor(Pose pose)
    {
        if (_anchor)
        {
            Destroy(_anchor);
            _anchor = null;
        }

        _anchor = Instantiate(_prefabAnchor, pose.position, pose.rotation);
        return _anchor.transform;
    }

    public override void SetFloor(PlaneId planeId, Vector3 position)
    {
        _planeId = planeId;
        _floorLevel = position;
    }
}

using System;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public class FloorManagerEditor : FloorManagerBase
{
    public override event Action<float> OnFloorLevelChanged;

    private GameObject _prefabAnchor;
    private EditorPlaneBehaviour _plane;
    private Vector3 _floorLevel;
    private bool _enableColliders;
    private PlaneId _planeId;
    private GameObject _anchor;
    private bool _isManualFloor;

    public override float floorLevel => _floorLevel.y;

    public override bool isFloorDetected => true;

    public override bool isManualFloor => _isManualFloor;

    public GameObject prefabAnchor
    {
        get => _prefabAnchor;
        set => _prefabAnchor = value;
    }

    public override Task<bool> InitializationAsync(IViewManager viewManager, PlaneManagerWrapper planeManager)
    {
        _planeId = new PlaneId(1, 1);
        _floorLevel = new Vector3(0, _DEFAULT_FLOOR_LEVEL, 0);
        _isManualFloor = false;
        OnFloorLevelChanged?.Invoke(_floorLevel.y);
        return Task.FromResult(true);
    }

    public override Task<bool> ResetAsync()
    {
        _floorLevel = new Vector3(0, _DEFAULT_FLOOR_LEVEL, 0);
        _planeId = new PlaneId(1, 1);
        _isManualFloor = false;
        OnFloorLevelChanged?.Invoke(_floorLevel.y);
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
        _isManualFloor = true;
        _planeId = planeId;
        _floorLevel = position;
        OnFloorLevelChanged?.Invoke(_floorLevel.y);
    }
}

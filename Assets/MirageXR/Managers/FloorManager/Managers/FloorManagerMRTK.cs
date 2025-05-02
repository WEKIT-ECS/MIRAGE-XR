using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public class FloorManagerMRTK : FloorManagerBase
{
    private const string AR_ANCHOR = "ARAnchor";

    private GameObject _anchor;
    private GlobalClickListener _globalClickListener;
    private Vector3 _floorLevel;
    private bool _isFloorDetected;

    public override float floorLevel => _isFloorDetected ? _floorLevel.y : _DEFAULT_FLOOR_LEVEL;

    public override bool isFloorDetected => _isFloorDetected;

    public override Task<bool> InitializationAsync(IViewManager viewManager, PlaneManagerWrapper planeManager)
    {
        return Task.FromResult(true);
    }

    public override Task<bool> ResetAsync()
    {
        return Task.FromResult(true);
    }

    public override Transform CreateAnchor(Pose pose)
    {
        if (_anchor)
        {
            Destroy(_anchor);
            _anchor = null;
        }

        _anchor = new GameObject(AR_ANCHOR);
        _anchor.SetPose(pose);

        return _anchor.transform;
    }

    public override void SetFloor(PlaneId planeId, Vector3 position)
    {
        _floorLevel = position;
        _isFloorDetected = true;
    }
}

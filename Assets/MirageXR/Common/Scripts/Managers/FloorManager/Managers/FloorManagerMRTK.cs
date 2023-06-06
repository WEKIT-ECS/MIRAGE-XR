using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using Action = System.Action;

public class FloorManagerMRTK : FloorManagerBase
{
    private GameObject _anchor;

    public override bool enableColliders => false;

    public override bool showPlanes => false;

    public override float floorLevel => _DEFAULT_FLOOR_LEVEL;

    public override bool isFloorDetected => false;

    public override Task<bool> InitializationAsync()
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

        _anchor = new GameObject("ARAnchor");
        _anchor.SetPose(pose);

        return _anchor.transform;
    }

    public override void SetFloor(IPlaneBehaviour floor)
    {
    }

    public override void EnableFloorDetection(Action onFloorDetected)
    {
        onFloorDetected?.Invoke();
    }

    public override void DisableFloorDetection()
    {
    }

    public override void Dispose()
    {
    }
}

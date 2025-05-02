using System;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public abstract class FloorManagerBase : MonoBehaviour, IFloorManager
{
    protected const float _DEFAULT_FLOOR_LEVEL = -1.7f;

    public abstract float floorLevel { get; }

    public abstract bool isFloorDetected { get; }

    public abstract Task<bool> InitializationAsync(IViewManager viewManager, PlaneManagerWrapper planeManager);

    public abstract Task<bool> ResetAsync();

    public abstract Transform CreateAnchor(Pose pose);

    public abstract void SetFloor(PlaneId planeId, Vector3 position);
}

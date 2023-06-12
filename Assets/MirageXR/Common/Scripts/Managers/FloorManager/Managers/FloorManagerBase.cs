using System;
using System.Threading.Tasks;
using UnityEngine;

public abstract class FloorManagerBase : MonoBehaviour, IFloorManager
{
    protected const float _DEFAULT_FLOOR_LEVEL = -1.7f;

    protected Action _onFloorDetected;

    public abstract bool enableColliders { get; }

    public abstract bool showPlanes { get; }

    public abstract float floorLevel { get; }

    public abstract bool isFloorDetected { get; }

    public abstract Task<bool> InitializationAsync();

    public abstract Task<bool> ResetAsync();

    public abstract Transform CreateAnchor(Pose pose);

    public abstract void SetFloor(IPlaneBehaviour floor);

    public abstract void EnableFloorDetection(Action onFloorDetected);

    public abstract void DisableFloorDetection();

    public abstract void Dispose();
}
